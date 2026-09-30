using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Loads;
using St7API;

namespace Rhino2Straus.Import;

internal sealed partial class NativeModelReader
{
    private sealed record AttributeInstance(int Kind,string Name,int Local,int Axis,int Case,int Id);
    private IEnumerable<AttributeInstance> Attributes(int entity,int number)
    {
        // The R31 attribute identifiers are distinct from similarly named display enums.
        var fields=typeof(St7).GetFields(BindingFlags.Public|BindingFlags.Static).Where(f=>f.IsLiteral&&f.FieldType==typeof(int)&&
            (entity==St7.tyNODE ? new[]{"aoRestraint","aoForce","aoMoment","aoTemperature","aoMTranslation","aoMRotation","aoKTranslation","aoKRotation","aoDamping","aoNSMass","aoNodeInfluence","aoNodeHeatSource","aoNodeVelocity","aoNodeAcceleration"}.Contains(f.Name)
            : entity==St7.tyBEAM ? f.Name.StartsWith("aoBeam")||f.Name.StartsWith("aoPipe")||f.Name=="aoCableFreeLength"
            : entity==St7.tyPLATE ? f.Name.StartsWith("aoPlate")||f.Name=="aoLoadPatch" : f.Name.StartsWith("aoBrick")))
            .GroupBy(f=>(int)f.GetRawConstantValue()).Select(g=>g.First());
        foreach(var f in fields)
        {
            int kind=(int)f.GetRawConstantValue(),count=0;
            Check(St7.St7GetEntityAttributeSequenceCount(id,entity,number,kind,ref count),"Count "+f.Name);
            if(count==0)continue;
            if(count<0||count>1000000)throw new System.IO.InvalidDataException("Invalid attribute count");
            var values=new int[checked(count*4)];Check(St7.St7GetEntityAttributeSequence(id,entity,number,kind,count,values),"Read "+f.Name);
            for(int i=0;i<count;i++)yield return new(kind,f.Name,values[4*i],values[4*i+1],values[4*i+2],values[4*i+3]);
        }
    }
    private void ReadAttributes(int entity,int number,Action<AttributeInstance> read)
    {
        foreach(var a in Attributes(entity,number))
        {
            try {read(a);}
            catch(NotSupportedException ex){Omit($"Entity {entity}:{number}, {a.Name}, case {a.Case}",ex.Message);}
        }
    }
    private static void GlobalUcs(int ucs)
    {if(ucs!=1)throw new NotSupportedException($"UCS {ucs} is not global; oriented restraint/stiffness/mass cannot be represented by diagonal global values.");}
    private static void Unsupported(AttributeInstance a) => throw new NotSupportedException($"{a.Name} is not represented in the Grasshopper Model.");

    private void ReadNodeAttributes(int n,NodeElementModel node) => ReadAttributes(St7.tyNODE,n,a=>
    {
        int ucs=1;
        switch(a.Kind)
        {
            case St7.aoRestraint:
                if(a.Case!=freedom)return;
                var status=new int[6];var disp=new double[6];
                Check(St7.St7GetNodeRestraint6(id,n,freedom,ref ucs,status,disp),"Read restraint");GlobalUcs(ucs);
                node.Support=new NodeSupportModel(status[0]!=0,status[1]!=0,status[2]!=0,status[3]!=0,status[4]!=0,status[5]!=0);node.PrescribedDisplacement=disp;break;
            case St7.aoKTranslation:
            case St7.aoKRotation:
                if(a.Case!=freedom)return;
                var k=new double[3];Check(a.Kind==St7.aoKTranslation?St7.St7GetNodeKTranslation3F(id,n,freedom,ref ucs,k):St7.St7GetNodeKRotation3F(id,n,freedom,ref ucs,k),"Read nodal spring");GlobalUcs(ucs);
                if(a.Kind==St7.aoKTranslation)node.TranslationSpring=Vector(k);else node.RotationSpring=Vector(k);break;
            case St7.aoMTranslation:node.TranslationMass=Vector(Values(3,v=>St7.St7GetNodeTMass3(id,n,v),"Read nodal mass"));break;
            case St7.aoMRotation:
                var mass=new double[3];Check(St7.St7GetNodeRMass3(id,n,ref ucs,mass),"Read rotational mass");GlobalUcs(ucs);node.RotationMass=Vector(mass);break;
            case St7.aoForce:
            case St7.aoMoment:
                var v=Values(3,v=>a.Kind==St7.aoForce?St7.St7GetNodeForce3(id,n,a.Case,v):St7.St7GetNodeMoment3(id,n,a.Case,v),"Read nodal load");
                node.NodalLoadList.Add(a.Kind==St7.aoForce?new(cases[a.Case],v[0],v[1],v[2],0,0,0):new(cases[a.Case],0,0,0,v[0],v[1],v[2]));break;
            case St7.aoTemperature:
                int temperatureType=0;Check(St7.St7GetNodeTemperatureType1(id,n,a.Case,ref temperatureType),"Read temperature type");
                if(temperatureType<0||temperatureType>2)throw new NotSupportedException("Table-based nodal temperatures are not supported.");
                node.Temperatures.Add(new(){LoadCase=cases[a.Case],Type=temperatureType,Temperature=Values(1,v=>St7.St7GetNodeTemperature1(id,n,a.Case,v),"Read temperature")[0]});break;
            default:Unsupported(a);break;
        }
    });

    private void ReadBeamAttributes(int n,FrameElementModel beam) => ReadAttributes(St7.tyBEAM,n,a=>
    {
        int type=0,project=0;
        switch(a.Kind)
        {
            case St7.aoBeamAngle:break; // Initial axis system already read, including reference node/angle.
            case St7.aoBeamOffset:
                var offset=Values(2,v=>St7.St7GetBeamOffset2(id,n,v),"Read beam offset");beam.Offset=new(offset[0],offset[1],0);break;
            case St7.aoBeamTEndRelease:
            case St7.aoBeamREndRelease:
                var flags=new int[3];var springs=new double[3];bool translation=a.Kind==St7.aoBeamTEndRelease;
                Check(translation?St7.St7GetBeamTRelease3(id,n,a.Local,flags,springs):St7.St7GetBeamRRelease3(id,n,a.Local,flags,springs),"Read beam release");
                if(flags.Any(f=>f!=St7.brFixed&&f!=St7.brReleased))throw new NotSupportedException("Spring/nonlinear releases are not boolean releases.");
                var target=a.Local==1?beam.StartReleases:beam.EndReleases;for(int i=0;i<3;i++)target[i+(translation?0:3)]=flags[i]==St7.brReleased;break;
            case St7.aoBeamPreTension:
                var pre=new double[1];Check(St7.St7GetBeamPreLoad1(id,n,a.Case,ref type,pre),"Read beam preload");
                if(type!=St7.plBeamPreStrain&&type!=St7.plBeamPreTension)throw new NotSupportedException("Unsupported beam preload type.");
                beam.FrameLoadList.Add(new FramePreloadModel{LoadCase=cases[a.Case],Value=pre[0],Strain=type==St7.plBeamPreStrain});break;
            case St7.aoBeamTempGradient:
                var gradient=Values(2,v=>St7.St7GetBeamTempGradient2(id,n,a.Case,v),"Read beam gradient");
                beam.FrameLoadList.Add(new FrameTemperatureGradientModel{LoadCase=cases[a.Case],Gradient1=gradient[0],Gradient2=gradient[1]});break;
            case St7.aoBeamCFL:case St7.aoBeamCFG:case St7.aoBeamCML:case St7.aoBeamCMG:
                var point=new double[4];bool local=a.Kind is St7.aoBeamCFL or St7.aoBeamCML;bool moment=a.Kind is St7.aoBeamCML or St7.aoBeamCMG;
                Check(moment?(local?St7.St7GetBeamPointMomentPrincipal4ID(id,n,a.Case,a.Id,point):St7.St7GetBeamPointMomentGlobal4ID(id,n,a.Case,a.Id,point))
                    :(local?St7.St7GetBeamPointForcePrincipal4ID(id,n,a.Case,a.Id,point):St7.St7GetBeamPointForceGlobal4ID(id,n,a.Case,a.Id,point)),"Read beam point load");
                beam.FrameLoadList.Add(new FramePointLoadModel{LoadCase=cases[a.Case],Value=Vector(point),Position=point[3],Moment=moment,CoordinateSystem=local?CoordinateSystemModel.Local:CoordinateSystemModel.Global});break;
            case St7.aoBeamDLL:case St7.aoBeamDLG:case St7.aoBeamDML:
                var dl=new double[6];
                Check(a.Kind==St7.aoBeamDLG?St7.St7GetBeamDistributedForceGlobal6ID(id,n,a.Axis,a.Case,a.Id,ref project,ref type,dl)
                    :a.Kind==St7.aoBeamDLL?St7.St7GetBeamDistributedForcePrincipal6ID(id,n,a.Axis,a.Case,a.Id,ref type,dl)
                    :St7.St7GetBeamDistributedMomentPrincipal6ID(id,n,a.Axis,a.Case,a.Id,ref type,dl),"Read distributed beam load");
                if(type<0||type>5||a.Axis<1||a.Axis>3)throw new NotSupportedException("Unsupported distributed load shape or axis.");
                beam.FrameLoadList.Add(new FrameLoadStrausModel{LoadCase=cases[a.Case],LoadSchema=(FrameLoadStrausModel.LoadSchemas)type,
                    Direction=(FrameLoadStrausModel.LoadDirections)(a.Axis-1),LoadType=a.Kind==St7.aoBeamDML?FrameLoadStrausModel.FrameLoadTypes.Moment:FrameLoadStrausModel.FrameLoadTypes.Force,
                    CoordinateSystem=a.Kind==St7.aoBeamDLG?CoordinateSystemModel.Global:CoordinateSystemModel.Local,IsProjected=project!=0,
                    PA=dl[0],PB=dl[1],P1=dl[2],P2=dl[3],A=dl[4],B=type==St7.dlTrapezoidal?dl[5]:1-dl[5]});break;
            default:Unsupported(a);break;
        }
    });

    private void ReadPlateAttributes(int n,AreaElementModel plate) => ReadAttributes(St7.tyPLATE,n,a=>
    {
        int type=0;
        switch(a.Kind)
        {
            case St7.aoPlateAngle:break;
            case St7.aoPlateOffset:plate.Offset=Values(1,v=>St7.St7GetPlateOffset1(id,n,v),"Read plate offset")[0];break;
            case St7.aoPlateThickness:
                var t=Values(2,v=>St7.St7GetPlateThickness2(id,n,v),"Read element thickness");
                var section=new AreaThicknessModel($"Plate {n} thickness",t[0],t[1],0){Id=++nextPlateProperty};
                plate.AreaProperty=new AreaPropertyModel($"Plate {n} override",plate.Material,section,plate.AreaProperty.PropertyType){Id=nextPlateProperty};
                plate.AreaThickness=section;break;
            case St7.aoPlatePreLoad:
                var pre=new double[3];Check(St7.St7GetPlatePreLoad3(id,n,a.Case,ref type,pre),"Read plate preload");
                if(type!=St7.plPlatePreStrain&&type!=St7.plPlatePreStress)throw new NotSupportedException("Unsupported plate preload type.");
                plate.AreaLoadList.Add(new AreaPreloadModel{LoadCase=cases[a.Case],Value=Vector(pre),Strain=type==St7.plPlatePreStrain});break;
            case St7.aoPlateTempGradient:plate.AreaLoadList.Add(new AreaTemperatureGradientModel{LoadCase=cases[a.Case],Gradient=Values(1,v=>St7.St7GetPlateTempGradient1(id,n,a.Case,v),"Read plate gradient")[0]});break;
            case St7.aoPlateFacePressure:
                var p=Values(2,v=>St7.St7GetPlateNormalPressure2(id,n,a.Case,v),"Read normal pressure");
                if(p[1]!=0)throw new NotSupportedException("Pressure on the plus-Z surface is not represented by the single normal-pressure attribute.");
                plate.AreaLoadList.Add(new AreaPressureLoadModel(cases[a.Case],p[0]));break;
            case St7.aoPlateGlobalPressure:
                var g=new double[3];Check(St7.St7GetPlateGlobalPressure3S(id,n,a.Local,a.Case,ref type,g),"Read global pressure");
                plate.AreaLoadList.Add(new AreaGlobalPressureStraus(cases[a.Case],a.Local==St7.psPlatePlusZ?AreaGlobalPressureStraus.LoadFace.Top:AreaGlobalPressureStraus.LoadFace.Bottom,CoordinateSystemModel.Global,g[0],g[1],g[2],type!=0));break;
            case St7.aoPlateEdgeNormalPressure:
                var edge=Values(1,v=>St7.St7GetPlateEdgePressure1(id,n,a.Case,a.Local,v),"Read edge pressure");
                plate.AreaLoadList.Add(new AreaEdgePressureModel{LoadCase=cases[a.Case],Edge=a.Local,Value=edge[0]});break;
            default:Unsupported(a);break;
        }
    });

    private void ReadSolidAttributes(int n,SolidElementModel solid) => ReadAttributes(St7.tyBRICK,n,a=>
    {
        if(a.Kind==St7.aoBrickPressure)solid.Pressures.Add(new(cases[a.Case],a.Local,Values(1,v=>St7.St7GetBrickNormalPressure1(id,n,a.Local,a.Case,v),"Read solid pressure")[0]));
        else Unsupported(a);
    });
}
