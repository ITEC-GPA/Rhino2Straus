using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rhino.Geometry;
using Rhino2Straus.Api;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Settings;
using St7API;

namespace Rhino2Straus.Core.Helper;

public static class BxsSections
{
    public static FrameSectionModel Read(string path, string name, ModelUnitsModel units)
    {
        path=Path.GetFullPath(path);
        if(!File.Exists(path)||!path.EndsWith(".bxs",StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Supply an existing Straus7 BXS section file.");
        string model=Path.Combine(Path.GetTempPath(),"Rhino2Straus-section-"+Guid.NewGuid().ToString("N")+".st7");
        try
        {
            using var session=new StrausSession(model,create:true);int id=session.Id;
            void Check(int code)=>StrausRuntime.Check(code,"Read BXS section");
            Check(St7.St7SetUnits(id,StrausUnits.ToApi(units)));Check(St7.St7NewBeamProperty(id,1,St7.btBeam,name));Check(St7.St7AssignBXS(id,1,path));
            var data=new double[11];Check(St7.St7GetBeamSectionPropertyData(id,1,new int[1],data));
            var result=new FrameSectionModel{Name=name,SectionGeometryType=FrameSectionModel.SectionGeometryTypes.Generic,Offset=FrameSectionModel.OffsetTypes.CC,
                SectionArea=data[St7.ipAREA],I11=data[St7.ipI11],I22=data[St7.ipI22],J=data[St7.ipJ],ShearL1=data[St7.ipSL1],ShearL2=data[St7.ipSL2],ShearA1=data[St7.ipSA1],ShearA2=data[St7.ipSA2],Centroid=new Point2d(data[St7.ipXBAR],data[St7.ipYBAR]),Angle=data[St7.ipANGLE],BxsData=File.ReadAllBytes(path),BxsLengthUnit=units.LengthUnit};
            int loops=0,plates=0;Check(St7.St7GetNumBXSLoopsAndPlates(id,1,ref loops,ref plates));var curves=new List<Curve>();
            for(int loop=1;loop<=loops;loop++)
            {
                int count=0;Check(St7.St7GetNumBXSLoopPoints(id,1,loop,ref count));if(count<3||count>1000000)throw new InvalidDataException("Invalid BXS loop size.");
                var xy=new double[checked(2*count)];Check(St7.St7GetBXSLoop(id,1,loop,count,ref count,xy));
                var points=Enumerable.Range(0,count).Select(i=>new Point3d(xy[2*i]-result.Centroid.X,xy[2*i+1]-result.Centroid.Y,0)).ToList();
                if(points[0]!=points[^1])points.Add(points[0]);curves.Add(new PolylineCurve(points));
            }
            result.Breps=Brep.CreatePlanarBreps(curves,Math.Max(1e-10,Math.Sqrt(result.SectionArea)*1e-8))?.ToList()??new();return result;
        }
        finally{if(File.Exists(model))File.Delete(model);}
    }

    internal static void Assign(int id,int property,byte[] data)
    {
        string path=Path.Combine(Path.GetTempPath(),"Rhino2Straus-section-"+Guid.NewGuid().ToString("N")+".bxs");
        try{File.WriteAllBytes(path,data);StrausRuntime.Check(St7.St7AssignBXS(id,property,path),"Assign embedded BXS section");}
        finally{if(File.Exists(path))File.Delete(path);}
    }
}
