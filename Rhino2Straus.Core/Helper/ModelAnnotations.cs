using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.Loads;
using Rhino2Straus.Core.Models;
using Rhino2Straus.Core.Settings;

namespace Rhino2Straus.Core.Helper;

public enum PreviewSymbol { Arrow, Moment, Support, Release, Spring, Mass, Text, Offset, Axis }
public sealed record PreviewMark(PreviewLayer Layer, string Owner, Point3d Point, Vector3d Vector, string Text,
    PreviewSymbol Symbol, Color Color, string CaseName=null);

/// <summary>Physical directions and positions of load/attribute glyphs. No native Straus7 calls.</summary>
public sealed class ModelAnnotations
{
    public List<PreviewMark> Marks { get; } = new();
    public List<string> Warnings { get; } = new();
    private readonly PreviewSettings settings;
    private readonly HashSet<string> cases;
    private const int MaximumMarks=25000;
    private readonly string forceUnit,lengthUnit,temperatureUnit;
    private static readonly Color LoadColor=Color.DarkOrange, AttributeColor=Color.MediumVioletRed;
    private static readonly Vector3d[] Global={Vector3d.XAxis,Vector3d.YAxis,Vector3d.ZAxis};
    private static readonly string[] Dofs={"UX","UY","UZ","RX","RY","RZ"};
    private static string N(double value)=>value.ToString("G4",CultureInfo.InvariantCulture);
    private static string V(Vector3d v)=>$"({N(v.X)}, {N(v.Y)}, {N(v.Z)})";
    private static Point3d Center(IEnumerable<Point3d> points) {var p=points.ToArray();return new(p.Average(x=>x.X),p.Average(x=>x.Y),p.Average(x=>x.Z));}
    private bool CaseVisible(string name)=>cases.Count==0||cases.Contains(name??"");
    private void Add(PreviewLayer layer,string owner,Point3d point,Vector3d vector,string text,PreviewSymbol symbol,string loadCase=null,Color? color=null)
    {
        if(!settings.Shows(layer)||loadCase!=null&&!CaseVisible(loadCase))return;
        if(!point.IsValid||!vector.IsValid)return;
        if(Marks.Count>=MaximumMarks){if(Warnings.Count==0)Warnings.Add("Preview limited to 25,000 symbols; filter load cases or hide categories.");return;}
        Marks.Add(new(layer,owner,point,vector,loadCase==null?text:$"{text} [{loadCase}]",symbol,color??(layer==PreviewLayer.Loads?LoadColor:AttributeColor),loadCase));
    }
    private void VectorLoad(string owner,Point3d point,Vector3d value,bool moment,string text,string loadCase)
    {
        if(value.SquareLength<=1e-30)return;
        Add(PreviewLayer.Loads,owner,point,value,text,moment?PreviewSymbol.Moment:PreviewSymbol.Arrow,loadCase);
    }
    private void TextLoad(string owner,Point3d point,string text,string loadCase)=>Add(PreviewLayer.Loads,owner,point,Vector3d.Zero,text,PreviewSymbol.Text,loadCase);
    private void Axes(string owner,Point3d p,Vector3d x,Vector3d y,Vector3d z)
    {
        Add(PreviewLayer.Axes,owner,p,x,"1",PreviewSymbol.Axis,color:Color.Red);
        Add(PreviewLayer.Axes,owner,p,y,"2",PreviewSymbol.Axis,color:Color.Green);
        Add(PreviewLayer.Axes,owner,p,z,"3",PreviewSymbol.Axis,color:Color.Blue);
    }
    private void Label(PreviewLayer layer,string owner,Point3d p,string text)=>Add(layer,owner,p,Vector3d.Zero,text,PreviewSymbol.Text,color:Color.DimGray);
    public ModelAnnotations(ModelModel model,PreviewSettings options,IEnumerable<string> caseNames=null)
    {
        settings=options;options.Validate();cases=new(caseNames?.Where(n=>!string.IsNullOrWhiteSpace(n)).Select(n=>n.Trim())??Array.Empty<string>(),StringComparer.Ordinal);
        var unknown=cases.Where(c=>!model.LoadCases.ContainsKey(c)).ToArray();if(unknown.Length>0)throw new ArgumentException("Unknown load cases: "+string.Join(", ",unknown));
        forceUnit=model.ModelUnits.ForceUnit==ModelUnitsModel.ForceUnitTypes.KN?"kN":"N";
        lengthUnit=model.ModelUnits.LengthUnit.ToString().ToLowerInvariant();temperatureUnit="°"+model.ModelUnits.TemperatureUnit;
        if(!options.Enabled)return;
        foreach(var n in model.NodeElements.Values)
        {
            string owner="N"+n.Id;var p=n.Position;
            Label(PreviewLayer.Labels,owner,p,owner);
            var restraint=new[]{n.Support.Dx,n.Support.Dy,n.Support.Dz,n.Support.Mx,n.Support.My,n.Support.Mz};
            for(int i=0;i<6;i++)
            {
                if(restraint[i])Add(PreviewLayer.Supports,owner,p,Global[i%3],$"{owner} fixed {Dofs[i]}",PreviewSymbol.Support);
                double imposed=n.PrescribedDisplacement[i];if(imposed!=0)Add(PreviewLayer.Prescribed,owner,p,Global[i%3]*imposed,$"{Dofs[i]}={N(imposed)} {(i<3?lengthUnit:"rad")}",i<3?PreviewSymbol.Arrow:PreviewSymbol.Moment);
            }
            AttributeVector(PreviewLayer.Springs,n.TranslationSpring,n.RotationSpring,PreviewSymbol.Spring,"K","Kr",$"{forceUnit}/{lengthUnit}",$"{forceUnit}·{lengthUnit}/rad");
            AttributeVector(PreviewLayer.Masses,n.TranslationMass,n.RotationMass,PreviewSymbol.Mass,"m","Jm","t",$"t·{lengthUnit}²");
            void AttributeVector(PreviewLayer layer,Vector3d translation,Vector3d rotation,PreviewSymbol kind,string prefix,string rPrefix,string unit,string rUnit)
            {
                for(int i=0;i<3;i++)
                {
                    if(translation[i]!=0)Add(layer,owner,p,Global[i],$"{prefix}{Dofs[i]}={N(translation[i])} {unit}",kind);
                    if(rotation[i]!=0)Add(layer,owner,p,Global[i],$"{rPrefix}{Dofs[i+3]}={N(rotation[i])} {rUnit}",kind);
                }
            }
            foreach(var l in n.NodalLoadList)
            {
                var f=new Vector3d(l.FX,l.FY,l.FZ);var m=new Vector3d(l.MX,l.MY,l.MZ);
                VectorLoad(owner,p,f,false,$"F={V(f)} {forceUnit}",l.LoadCase?.Name);
                VectorLoad(owner,p,m,true,$"M={V(m)} {forceUnit}·{lengthUnit}",l.LoadCase?.Name);
            }
            foreach(var t in n.Temperatures)TextLoad(owner,p,$"T={N(t.Temperature)} {temperatureUnit}",t.LoadCase?.Name);
        }
        foreach(var b in model.FrameElements.Values) Beam(b);
        foreach(var p in model.AreaElements.Values) Plate(p);
        foreach(var s in model.SolidElements.Values) Solid(s);
        foreach(var l in model.LinkElements.Values)
        {
            var p=Center(new[]{l.NodeStart.Position,l.NodeEnd.Position});string owner="L"+l.Id;
            Label(PreviewLayer.Labels,owner,p,owner);var property=l.LinkProperty;
            string text=property.Type.ToString();
            if(property.Type==Rhino2Straus.Core.ElementProperties.LinkPropertyModel.LinkPropertyTypes.GEN)text+=$" K={N(property.Kx)},{N(property.Ky)},{N(property.Kz)}; Kr={N(property.Rx)},{N(property.Ry)},{N(property.Rz)}";
            else if(property.Type==Rhino2Straus.Core.ElementProperties.LinkPropertyModel.LinkPropertyTypes.MASTER_SLAVE)text+=" DOF="+string.Join(",",property.Options.Dofs);
            else if(property.Type==Rhino2Straus.Core.ElementProperties.LinkPropertyModel.LinkPropertyTypes.RIGID)text+=" plane="+new[]{"XYZ","XY","YZ","ZX"}[property.Options.RigidPlane];
            else if(property.Type==Rhino2Straus.Core.ElementProperties.LinkPropertyModel.LinkPropertyTypes.SECTOR_SYMMETRY)text+=" axis="+property.Options.SymmetryAxis;
            Label(PreviewLayer.Properties,owner,p,text);
        }
        if(model.SelfWeight is {} sw && model.NodeElements.Count>0)
        {
            var origin=Center(model.NodeElements.Values.Select(n=>n.Position));var direction=Global[(int)sw.GravityDirection]*sw.GravityValue;
            VectorLoad("Model",origin,direction,false,$"g={N(sw.GravityValue)} mm/s²; structural={sw.StructuralMassAcceleration}, NSM={sw.NonStructuralMassAcceleration}",sw.LoadCase?.Name);
        }
    }
    private void Beam(FrameElementModel b)
    {
        string owner="B"+b.Id;var start=b.NodeStart.Position;var delta=b.NodeEnd.Position-start;var center=start+delta*.5;
        b.GetLocalAxes(true,out var z,out var y,out var x,out _);Axes(owner,center,x,y,z);
        var offset=b.GetEffectiveOffset();var shift=x*offset.X+y*offset.Y;
        if(shift.SquareLength>1e-30)foreach(var p in new[]{start,b.NodeEnd.Position})Add(PreviewLayer.Offsets,owner,p,shift,$"offset={V(shift)} {lengthUnit}",PreviewSymbol.Offset);
        Label(PreviewLayer.Labels,owner,center,owner);Label(PreviewLayer.Properties,owner,center,b.FrameProperty.Name+" / "+b.Material.Name+" / "+b.FrameSection.Name);
        foreach(var end in new[]{(p:start,flags:b.StartReleases),(p:b.NodeEnd.Position,flags:b.EndReleases)})
            if(end.flags.Any(v=>v))Add(PreviewLayer.Releases,owner,end.p+shift,z,"Release "+string.Join(",",Dofs.Where((_,i)=>end.flags[i])),PreviewSymbol.Release);
        Vector3d ToWorld(Vector3d value,CoordinateSystemModel cs)=>cs==CoordinateSystemModel.Local?x*value.X+y*value.Y+z*value.Z:value;
        foreach(var load in b.FrameLoadList)
        {
            string c=load.LoadCase?.Name;if(!CaseVisible(c))continue;
            if(load is FramePointLoadModel point)
            {
                var value=ToWorld(point.Value,point.CoordinateSystem);
                VectorLoad(owner,start+delta*point.Position+shift,value,point.Moment,$"{(point.Moment?"M":"F")}={V(point.Value)} {(point.Moment?$"{forceUnit}·{lengthUnit}":forceUnit)} ({(point.CoordinateSystem==CoordinateSystemModel.Local?"local":"global")})",c);
            }
            else if(load is FrameLoadStrausModel distributed)
            {
                int axis=(int)distributed.Direction;var direction=axis is 0 or 3?Global[0]:axis is 1 or 4?Global[1]:Global[2];
                if(axis>=6)direction=-direction;direction=ToWorld(direction,distributed.CoordinateSystem);
                bool projected=distributed.IsProjected||axis is 3 or 4 or 5 or 7;
                double factor=projected?Vector3d.CrossProduct(z,direction).Length:1;
                var knots=BeamLoadDistribution.Knots(distributed);
                for(int segment=0;segment<knots.Count-1;segment++)for(int i=0;i<=4;i++)
                {
                    if(segment>0&&i==0)continue;var a=knots[segment];var v=knots[segment+1];double t=i/4d;
                    double position=a.Position+(v.Position-a.Position)*t,q=a.Value+(v.Value-a.Value)*t;
                    VectorLoad(owner,start+delta*position+shift,direction*(q*factor),distributed.LoadType==FrameLoadStrausModel.FrameLoadTypes.Moment,
                        $"q={N(q)} {(distributed.LoadType==FrameLoadStrausModel.FrameLoadTypes.Moment?forceUnit:forceUnit+"/"+lengthUnit)}{(projected?" projected":"")}",c);
                }
            }
            else if(load is FrameTemperatureGradientModel gradient)TextLoad(owner,center+shift,$"dT1={N(gradient.Gradient1)}, dT2={N(gradient.Gradient2)} {temperatureUnit}/{lengthUnit}",c);
            else if(load is FramePreloadModel preload)TextLoad(owner,center+shift,$"{(preload.Strain?"pre-strain":"pre-tension")}={N(preload.Value)} {(preload.Strain?"":forceUnit)}",c);
        }
    }
    private void Plate(AreaElementModel p)
    {
        string owner="P"+p.Id;var points=p.NodeList.Select(n=>n.Position).ToArray();var center=Center(points);
        p.GetLocalAxes(out var x,out var y,out var z);Axes(owner,center,x,y,z);
        double offset=p.Offset+p.AreaThickness.Offset,half=p.AreaThickness.ThicknessMembrane/2;
        if(offset!=0)Add(PreviewLayer.Offsets,owner,center,z*offset,$"offset={N(offset)} {lengthUnit}",PreviewSymbol.Offset);
        Label(PreviewLayer.Labels,owner,center,owner);Label(PreviewLayer.Properties,owner,center,$"{p.AreaProperty.Name} / {p.Material.Name}; t={N(p.AreaThickness.ThicknessMembrane)} {lengthUnit}");
        foreach(var load in p.AreaLoadList)
        {
            string c=load.LoadCase?.Name;if(!CaseVisible(c))continue;
            if(load is AreaPressureLoadModel normal)Pressure(z*normal.Value,offset-half,$"p={N(normal.Value)} {forceUnit}/{lengthUnit}² (-z face)");
            else if(load is AreaGlobalPressureStraus global)
            {
                var value=global.Value;
                if(global.Projected)value=new(value.X*Math.Abs(z.X),value.Y*Math.Abs(z.Y),value.Z*Math.Abs(z.Z));
                Pressure(value,offset+(global.Face==AreaGlobalPressureStraus.LoadFace.Top?half:-half),$"p={V(global.Value)} {forceUnit}/{lengthUnit}²{(global.Projected?" projected":"")}");
            }
            else if(load is AreaEdgePressureModel edge)
            {
                int index=edge.Edge-1;if(index<0||index>=points.Length)continue;
                var a=points[index];var b=points[(index+1)%points.Length];var d=Vector3d.CrossProduct(b-a,z);d.Unitize();var mid=Center(new[]{a,b});if(d*(mid-center)<0)d=-d;
                for(int i=0;i<3;i++)VectorLoad(owner,a+(b-a)*(i+1)/4+z*offset,d*edge.Value,false,$"edge {edge.Edge}: p={N(edge.Value)} {forceUnit}/{lengthUnit}²",c);
            }
            else if(load is AreaTemperatureGradientModel thermal)TextLoad(owner,center+z*offset,$"dT={N(thermal.Gradient)} {temperatureUnit}/{lengthUnit}",c);
            else if(load is AreaPreloadModel pre)TextLoad(owner,center+z*offset,$"{(pre.Strain?"pre-strain":"pre-stress")}={V(pre.Value)} {(pre.Strain?"":"kPa")}",c);
            void Pressure(Vector3d value,double elevation,string text)
            {
                foreach(var sample in points.Select(v=>center+(v-center)*.6).Append(center))VectorLoad(owner,sample+z*elevation,value,false,text,c);
            }
        }
    }
    public static int[] SolidFace(int nodeCount,int face)
    {
        int[][] faces=nodeCount switch
        {
            4=>new[]{new[]{0,1,2},new[]{3,0,2},new[]{3,1,0},new[]{3,2,1}},
            6=>new[]{new[]{0,1,2},new[]{4,1,0,3},new[]{3,5,4},new[]{3,0,2,5},new[]{5,2,1,4}},
            8=>new[]{new[]{0,1,2,3},new[]{6,2,1,5},new[]{5,4,7,6},new[]{4,0,3,7},new[]{7,3,2,6},new[]{5,1,0,4}},
            _=>throw new ArgumentException("Use Tet4, Wedge6 or Hexa8.")
        };
        if(face<1||face>faces.Length)throw new ArgumentException("Invalid native solid face.");return faces[face-1];
    }
    private void Solid(SolidElementModel s)
    {
        string owner="S"+s.Id;var center=Center(s.Nodes.Select(n=>n.Position));
        Label(PreviewLayer.Labels,owner,center,owner);Label(PreviewLayer.Properties,owner,center,s.Material.Name);
        foreach(var p in s.Pressures)
        {
            var points=SolidFace(s.Nodes.Count,p.Face).Select(i=>s.Nodes[i].Position).ToArray();var faceCenter=Center(points);
            var inward=Vector3d.CrossProduct(points[1]-points[0],points[2]-points[0]);inward.Unitize();if(inward*(center-faceCenter)<0)inward=-inward;
            VectorLoad(owner,faceCenter,inward*p.Value,false,$"face {p.Face}: p={N(p.Value)} {forceUnit}/{lengthUnit}²",p.LoadCase?.Name);
        }
    }
}
