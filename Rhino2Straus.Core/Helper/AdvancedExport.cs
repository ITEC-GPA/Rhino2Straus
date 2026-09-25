using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using Rhino2Straus.Api;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Loads;
using Rhino2Straus.Core.Models;
using St7API;

namespace Rhino2Straus.Core.Helper;

internal static class AdvancedExport
{
    private static double[] V(Vector3d v) => new[] { v.X, v.Y, v.Z };
    private static void Check(int code) => StrausRuntime.Check(code, "Write Straus7 attribute");

    public static void AlignBeam(int id, int number, FrameElementModel beam)
    {
        // Query the native zero orientation, including vertical beams, instead of assuming a fallback axis.
        var axes=new double[9];Check(St7.St7GetBeamAxisSystemInitial(id,number,axes));
        var first=new Vector3d(axes[0],axes[1],axes[2]);
        beam.GetLocalAxes(true,out var longitudinal,out _,out var desired,out _);
        double angle=Math.Atan2(Vector3d.Multiply(longitudinal,Vector3d.CrossProduct(first,desired)),Vector3d.Multiply(first,desired));
        Check(St7.St7SetBeamReferenceAngle1(id,number,new[]{angle*180/Math.PI}));
    }

    public static void Node(int id, int number, NodeElementModel node, Dictionary<string, int> cases)
    {
        if (node.TranslationSpring != Vector3d.Zero) Check(St7.St7SetNodeKTranslation3F(id, number, 1, 1, V(node.TranslationSpring)));
        if (node.RotationSpring != Vector3d.Zero) Check(St7.St7SetNodeKRotation3F(id, number, 1, 1, V(node.RotationSpring)));
        if (node.TranslationMass != Vector3d.Zero) Check(St7.St7SetNodeTMass3(id, number, V(node.TranslationMass)));
        if (node.RotationMass != Vector3d.Zero) Check(St7.St7SetNodeRMass3(id, number, 1, V(node.RotationMass)));
        foreach (var temp in node.Temperatures)
            Check(St7.St7SetNodeTemperature1(id, number, cases[temp.LoadCase.Name], new[] { temp.Temperature }));
    }

    public static void FrameAttributes(int id, int number, FrameElementModel beam)
    {
        foreach (int end in new[] { 1, 2 })
        {
            var r = end == 1 ? beam.StartReleases : beam.EndReleases;
            if (!r.Any(x => x)) continue;
            Check(St7.St7SetBeamTRelease3(id, number, end, r.Take(3).Select(x => x ? St7.brReleased : St7.brFixed).ToArray(), new double[3]));
            Check(St7.St7SetBeamRRelease3(id, number, end, r.Skip(3).Take(3).Select(x => x ? St7.brReleased : St7.brFixed).ToArray(), new double[3]));
        }
    }

    public static bool FrameLoad(int id, int number, int lc, int loadId, FrameLoadBaseModel load)
    {
        if (load is FramePointLoadModel point)
        {
            var values = new[] { point.Value.X, point.Value.Y, point.Value.Z, point.Position };
            bool local = point.CoordinateSystem == CoordinateSystemModel.Local;
            int code = point.Moment
                ? (local ? St7.St7SetBeamPointMomentPrincipal4ID(id, number, lc, loadId, values) : St7.St7SetBeamPointMomentGlobal4ID(id, number, lc, loadId, values))
                : (local ? St7.St7SetBeamPointForcePrincipal4ID(id, number, lc, loadId, values) : St7.St7SetBeamPointForceGlobal4ID(id, number, lc, loadId, values));
            Check(code); return true;
        }
        if (load is FrameTemperatureGradientModel thermal)
        { Check(St7.St7SetBeamTempGradient2(id, number, lc, new[] { thermal.Gradient1, thermal.Gradient2 })); return true; }
        if (load is FramePreloadModel pre)
        { Check(St7.St7SetBeamPreLoad1(id, number, lc, pre.Strain ? St7.plBeamPreStrain : St7.plBeamPreTension, new[] { pre.Value })); return true; }
        return false;
    }

    public static bool AreaLoad(int id, int number, int lc, AreaLoadBaseModel load)
    {
        if (load is AreaTemperatureGradientModel thermal)
        { Check(St7.St7SetPlateTempGradient1(id, number, lc, new[] { thermal.Gradient })); return true; }
        if (load is AreaPreloadModel pre)
        { Check(St7.St7SetPlatePreLoad3(id, number, lc, pre.Strain ? St7.plPlatePreStrain : St7.plPlatePreStress, V(pre.Value))); return true; }
        if (load is AreaEdgePressureModel edge)
        { Check(St7.St7SetPlateEdgePressure1(id, number, lc, edge.Edge, new[] { edge.Value })); return true; }
        return false;
    }

    public static void Solids(int id, ModelModel model, Dictionary<NodeElementModel, int> nodes, Dictionary<string,int> cases, Dictionary<ElementGroupModel,int> groups)
    {
        var properties=new Dictionary<MaterialModel,int>(); int number=0;
        foreach(var solid in model.SolidElements.Values)
        {
            var m=solid.Material;
            if(!properties.TryGetValue(m,out int property))
            {
                property=properties.Count+1; properties[m]=property;
                Check(St7.St7NewBrickProperty(id,property,St7.mtIsotropic,m.Name));
                Check(St7.St7SetBrickIsotropicMaterial(id,property,new[]{m.ModulusElasticity,m.PoissonRatio,m.Density/Math.Pow(StrausUnits.LengthPerMetre(model.ModelUnits.LengthUnit),3),StrausUnits.ThermalExpansion(m.ThermalCoefficient,model.ModelUnits),m.ViscousDamping,m.DampingRatio,m.Conductivity,m.SpecificHeat}));
            }
            number++;
            Check(St7.St7SetElementConnection(id,St7.tyBRICK,number,property,new[]{solid.Nodes.Count}.Concat(solid.Nodes.Select(n=>nodes[n])).ToArray()));
            Check(St7.St7SetBrickID(id,number,solid.Id));
            if(solid.Groups.Count>0)Check(St7.St7SetEntityGroup(id,St7.tyBRICK,number,groups[solid.Groups[0]]));
            foreach(var load in solid.Pressures.GroupBy(p=>(p.LoadCase.Name,p.Face)))
                Check(St7.St7SetBrickNormalPressure1(id,number,load.Key.Face,cases[load.Key.Name],new[]{load.Sum(p=>p.Value)}));
        }
    }

    public static void Links(int id, ModelModel model, Dictionary<NodeElementModel, int> nodes, Dictionary<ElementGroupModel, int> groups)
    {
        int number = 1, connection = 0;
        foreach (var link in model.LinkElements.Values)
        {
            if (link.LinkProperty.Type == LinkPropertyModel.LinkPropertyTypes.RIGID)
            {
                Check(St7.St7SetRigidLink(id, number, 1, St7.rlPlaneXYZ, new[] { 2, nodes[link.NodeStart], nodes[link.NodeEnd] }));
                  if (link.Groups.Count > 0) Check(St7.St7SetEntityGroup(id, St7.tyLINK, number, groups[link.Groups[0]]));
                Check(St7.St7SetLinkID(id, number++, link.Id));
            }
            else
            {
                var p=link.LinkProperty;int index=++connection;
                int property=model.FrameProperties.Count+index,beam=model.FrameElements.Count+index;
                Check(St7.St7NewBeamProperty(id,property,St7.btConnection,$"Connection {link.Id}"));
                Check(St7.St7SetConnectionData(id,property,new[]{p.Kx,p.Ky,p.Kz,p.Rx,p.Ry,p.Rz}));
                Check(St7.St7SetElementConnection(id,St7.tyBEAM,beam,property,new[]{2,nodes[link.NodeStart],nodes[link.NodeEnd]}));
                Check(St7.St7SetBeamConnectionUCS(id,beam,1,1));Check(St7.St7SetBeamConnectionUCS(id,beam,2,1));
                int label=checked(model.FrameElements.Values.Select(x=>x.Id).DefaultIfEmpty(0).Max()+link.Id);
                Check(St7.St7SetBeamID(id,beam,label));
                  if (link.Groups.Count > 0) Check(St7.St7SetEntityGroup(id, St7.tyBEAM, beam, groups[link.Groups[0]]));
            }
        }
    }
}
