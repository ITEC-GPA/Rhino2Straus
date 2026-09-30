using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rhino.Geometry;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Helper;
using St7API;

namespace Rhino2Straus.Import;

internal sealed partial class NativeModelReader
{
    private int nextPlateProperty;
    private MaterialModel Isotropic(string name, double[] v) => new()
    {
        Id=++materialId, Name=name, Type=MaterialModel.MaterialTypes.Isotropic, Standard=MaterialModel.Standards.Custom,
        ModulusElasticity=v[0], PoissonRatio=v[1], Density=v[2], ThermalCoefficient=v[3],
        ViscousDamping=v[4], DampingRatio=v[5], Conductivity=v[6], SpecificHeat=v[7]
    };

    private FramePropertyModel BeamProperty(int number)
    {
        if(beamProperties.TryGetValue(number,out var existing))return existing;
        var flags=new int[5];var sectionData=new double[St7.kNumBeamSectionData];var materialData=new double[St7.kNumMaterialData];
        Check(St7.St7GetBeamPropertyData(id,number,flags,sectionData,materialData),"Read beam property");
        var behaviour=flags[St7.ipBeamPropBeamType] switch
        {
            St7.btBeam=>FramePropertyModel.Behaviours.Beam,
            St7.btTruss=>FramePropertyModel.Behaviours.Truss,
            St7.btCable=>FramePropertyModel.Behaviours.Cable,
            _=>throw new NotSupportedException("Beam property type is not Beam, Truss, Cable or Connection.")
        };
        string name=PropertyName(St7.ptBEAMPROP,number);
        var m=Values(9,v=>St7.St7GetBeamMaterialData(id,number,v),"Read beam material");
        double nu=m[St7.ipBeamPoisson];
        if(flags[St7.ipBeamPropUsePoisson]==St7.btFalse)
        {
            if(m[St7.ipBeamShear]<=0)throw new NotSupportedException("Beam has invalid independent shear modulus.");
            nu=m[St7.ipBeamModulus]/(2*m[St7.ipBeamShear])-1;
        }
        var material=Isotropic(name,new[]{m[St7.ipBeamModulus],nu,m[St7.ipBeamDensity],m[St7.ipBeamAlpha],m[St7.ipBeamViscosity],m[St7.ipBeamDampingRatio],m[St7.ipBeamConductivity],m[St7.ipBeamSpecificHeat]});
        var s=sectionData;
        var section=new FrameSectionModel
        {
            Id=number,Name=name,SectionGeometryType=FrameSectionModel.SectionGeometryTypes.Generic,Offset=FrameSectionModel.OffsetTypes.CC,
            SectionArea=s[St7.ipAREA],I11=s[St7.ipI11],I22=s[St7.ipI22],J=s[St7.ipJ],ShearL1=s[St7.ipSL1],ShearL2=s[St7.ipSL2],
            ShearA1=s[St7.ipSA1],ShearA2=s[St7.ipSA2],Centroid=new Point2d(s[St7.ipXBAR],s[St7.ipYBAR]),Angle=s[St7.ipANGLE],Breps=new()
        };
        // Geometry is optional for user-defined numerical sections. Never infer a rectangle from A/I.
        string stem=Path.Combine(Path.GetTempPath(),"Rhino2Straus-import-section-"+Guid.NewGuid().ToString("N"));
        string mesh=stem+".st7",bxs=stem+".bxs";
        try
        {
            int meshCode=St7.St7SaveBeamSectionMesh(id,number,mesh);
            if(meshCode==0)
            {
                using(var session=new Rhino2Straus.Api.StrausSession(mesh,readOnly:false))
                    Check(St7.St7GenerateBXS(session.Id,bxs,new double[39]),"Extract section profile");
                var profile=BxsSections.Read(bxs,name,new());
                foreach(var brep in profile.Breps)brep.Translate(profile.Centroid.X-section.Centroid.X,profile.Centroid.Y-section.Centroid.Y,0);
                section.Breps=profile.Breps;
            }
            else data.Issues.Add(new($"Beam property {number}",$"Numerical section imported; native contour unavailable (API {meshCode}). No solid preview for this section.",false));
        }
        catch(InvalidOperationException ex){data.Issues.Add(new($"Beam property {number}","Numerical section imported without contour: "+ex.Message,false));}
        finally {foreach(string path in new[]{mesh,bxs})if(File.Exists(path))File.Delete(path);}
        var result=new FramePropertyModel(name,material,section){Id=number,Behaviour=behaviour};
        beamProperties[number]=result;return result;
    }

    private AreaPropertyModel PlateProperty(int number)
    {
        if(plateProperties.TryGetValue(number,out var existing))return existing;
        int kind=0,materialType=0;Check(St7.St7GetPlatePropertyType(id,number,ref kind,ref materialType),"Read plate property type");
        if(materialType!=St7.mtIsotropic)throw new NotSupportedException("Only isotropic plate materials are supported.");
        var type=kind switch
        {
            St7.ptPlaneStress=>AreaPropertyModel.PlatePropertyType.PlaneStress,St7.ptPlaneStrain=>AreaPropertyModel.PlatePropertyType.PlaneStrain,
            St7.ptAxisymmetric=>AreaPropertyModel.PlatePropertyType.Axisymmetric,St7.ptPlateShell=>AreaPropertyModel.PlatePropertyType.PlateShell,
            St7.ptShearPanel=>AreaPropertyModel.PlatePropertyType.ShearPanel,St7.ptMembrane=>AreaPropertyModel.PlatePropertyType.Membrane,
            St7.ptLoadPatch=>AreaPropertyModel.PlatePropertyType.LoadPatch,_=>throw new NotSupportedException("Unsupported plate formulation.")
        };
        string name=PropertyName(St7.ptPLATEPROP,number);
        var material=Isotropic(name,Values(8,v=>St7.St7GetPlateIsotropicMaterial(id,number,v),"Read plate material"));
        var t=Values(2,v=>St7.St7GetPlateThickness(id,number,v),"Read plate property thickness");
        var result=new AreaPropertyModel(name,material,new AreaThicknessModel(name,t[0],t[1],0){Id=++nextPlateProperty},type){Id=nextPlateProperty};
        plateProperties[number]=result;return result;
    }

    private MaterialModel SolidMaterial(int number)
    {
        if(solidMaterials.TryGetValue(number,out var existing))return existing;
        int type=0;Check(St7.St7GetBrickPropertyType(id,number,ref type),"Read solid property type");
        if(type!=St7.mtIsotropic)throw new NotSupportedException("Only isotropic solid materials are supported.");
        var material=Isotropic(PropertyName(St7.ptBRICKPROP,number),Values(8,v=>St7.St7GetBrickIsotropicMaterial(id,number,v),"Read solid material"));
        solidMaterials[number]=material;return material;
    }
}
