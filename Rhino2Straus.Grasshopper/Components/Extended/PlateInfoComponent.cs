using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Loads;
using Rhino2Straus.Core.Models;
using Rhino2Straus.Core.Settings;
using Rhino2Straus.Grasshopper.Datatype;
using Rhino2Straus.Import;
using St7API;

namespace Rhino2Straus.Grasshopper.Components.Extended;

public sealed class PlateInfoComponent : SafeComponent
{
    public PlateInfoComponent() : base("Deconstruct Plate", "Deconstruct Plate", "Deconstruct Plate", "Rhino2Straus", "09-Tools") { }
    public override Guid ComponentGuid => new Guid("47168338-0607-5929-a84c-8ae5b71e87d0");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddIntegerParameter("ID", "ID", "ID", GH_ParamAccess.item);
        p.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
        p.AddGenericParameter("Property", "Property", "Property", GH_ParamAccess.item);
        p.AddNumberParameter("Angle", "Angle", "Radians", GH_ParamAccess.item);
        p.AddNumberParameter("Offset", "Offset", "Offset", GH_ParamAccess.item);
        p.AddTextParameter("Groups", "Groups", "Groups", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var a=Item<GH_AreaElement>(da,0).Value; var mesh=new Mesh(); foreach(var n in a.NodeList) mesh.Vertices.Add(n.Position);
        if(a.NodeList.Count==3) mesh.Faces.AddFace(0,1,2); else if(a.NodeList.Count==4) mesh.Faces.AddFace(0,1,2,3); else throw new ArgumentException("Only Tri3/Quad4 plates are supported.");
        mesh.Normals.ComputeNormals(); da.SetData(0,a.Id); da.SetData(1,mesh); da.SetData(2,new GH_AreaProperty(a.AreaProperty)); da.SetData(3,a.Angle); da.SetData(4,a.Offset); da.SetDataList(5,a.Groups.Select(x=>x.GroupFullName));
    }
}
