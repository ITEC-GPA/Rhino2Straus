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

public sealed class DeformedNodesComponent : SafeComponent
{
    public DeformedNodesComponent() : base("Deformed Nodes", "Deformed Nodes", "Deformed Nodes", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("c804f4d9-8a14-572a-b976-66c40a5793fc");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddPointParameter("Points", "Points", "Points", GH_ParamAccess.list);
        p.AddVectorParameter("Displacements", "Displacements", "Displacements", GH_ParamAccess.list);
        p.AddNumberParameter("Scale", "Scale", "Scale", GH_ParamAccess.item, 1.0);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddPointParameter("Deformed", "Deformed", "Deformed", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var p=new List<Point3d>();var d=new List<Vector3d>();da.GetDataList(0,p);da.GetDataList(1,d);double scale=Item<double>(da,2);
        if(p.Count!=d.Count || !double.IsFinite(scale))throw new ArgumentException("Points and displacements must have matching counts and scale must be finite.");
        da.SetDataList(0,p.Select((x,i)=>x+d[i]*scale));
    }
}
