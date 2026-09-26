using System;
using System.IO;
using Rhino.Geometry;
using Rhino2Straus.Api;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Settings;
using St7API;

namespace Rhino2Straus.Core.Helper;

/// <summary>Generate native section properties and a discretised preview through documented R31 APIs.</summary>
public static class NativeSections
{
    public static void Assign(int id, int property, NativeSectionDefinition definition)
    {
        definition.Validate();
        void Check(int code) => StrausRuntime.Check(code,"Define native beam section");
        Check(definition.IsBgl ? St7.St7SetBeamSectionGeometryBGL(id,property,definition.Shape,definition.Dimensions)
            : St7.St7SetBeamSectionGeometry(id,property,definition.Shape,definition.Dimensions));
        Check(St7.St7SetBeamSectionNominalDiscretisation(id,property,new[]{definition.Options.Divisions,0,St7.btTrue}));
        Check(St7.St7SetBeamSectionCircularDiscretisation(id,property,new[]{definition.Options.CircularDivisions}));
        Check(St7.St7CalculateBeamSectionProperties(id,property,(byte)(definition.Options.IncludeShear ? St7.btTrue : St7.btFalse)));
    }

    public static FrameSectionModel Create(string name, bool bgl, int shape, double[] dimensions, ModelUnitsModel units, NativeSectionOptions options=null)
    {
        units ??= new ModelUnitsModel();
        var definition = new NativeSectionDefinition { IsBgl=bgl, Shape=shape, Dimensions=(double[])dimensions.Clone(), LengthUnit=units.LengthUnit, Options=(options??new()).Copy() };
        definition.Validate();
        if(string.IsNullOrWhiteSpace(name)) name=$"Straus {(bgl ? "BGL" : "section")} {shape}";
        string stem=Path.Combine(Path.GetTempPath(),"Rhino2Straus-profile-"+Guid.NewGuid().ToString("N"));
        string model=stem+".st7", mesh=stem+"-mesh.st7", bxs=stem+".bxs";
        var data=new double[11];
        try
        {
            void Check(int code) => StrausRuntime.Check(code,"Calculate native beam section");
            using(var session=new StrausSession(model,create:true))
            {
                Check(St7.St7SetUnits(session.Id,StrausUnits.ToApi(units)));
                Check(St7.St7NewBeamProperty(session.Id,1,St7.btBeam,name));
                Assign(session.Id,1,definition);
                Check(St7.St7GetBeamSectionPropertyData(session.Id,1,new int[1],data));
                // SaveBeamSectionMesh supports standard, BSL and BGL shapes; ExportBXS alone does not.
                Check(St7.St7SaveBeamSectionMesh(session.Id,1,mesh));
            }
            using(var session=new StrausSession(mesh,readOnly:false))
                Check(St7.St7GenerateBXS(session.Id,bxs,new double[39]));
            var result=BxsSections.Read(bxs,name,units);
            // BXS loops use the mesh centroid. Re-centre on the native property centroid before extrusion.
            foreach(var brep in result.Breps) brep.Translate(result.Centroid.X-data[St7.ipXBAR],result.Centroid.Y-data[St7.ipYBAR],0);
            result.SectionArea=data[St7.ipAREA]; result.I11=data[St7.ipI11]; result.I22=data[St7.ipI22]; result.J=data[St7.ipJ];
            result.ShearL1=data[St7.ipSL1]; result.ShearL2=data[St7.ipSL2]; result.ShearA1=data[St7.ipSA1]; result.ShearA2=data[St7.ipSA2];
            result.Centroid=new Point2d(data[St7.ipXBAR],data[St7.ipYBAR]); result.Angle=data[St7.ipANGLE];
            result.NativeDefinition=definition; result.BxsData=null;
            if(result.SectionArea<=0 || !double.IsFinite(result.SectionArea) || result.Breps.Count==0)
                throw new InvalidDataException("Straus7 returned an empty section or preview.");
            return result;
        }
        finally { foreach(var path in new[]{model,mesh,bxs}) if(File.Exists(path)) File.Delete(path); }
    }
}
