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

public sealed class BeamInfoComponent : SafeComponent
{
    public BeamInfoComponent() : base("Deconstruct Beam", "Deconstruct Beam", "Deconstruct Beam", ComponentTopics.Tab, ComponentTopics.Elements) { }
    public override Guid ComponentGuid => new Guid("eeb0768a-bd84-55cb-8395-0a917bde674a");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddIntegerParameter("ID", "ID", "ID", GH_ParamAccess.item);
        p.AddLineParameter("Line", "Line", "Line", GH_ParamAccess.item);
        p.AddGenericParameter("Property", "Property", "Property", GH_ParamAccess.item);
        p.AddNumberParameter("Length", "Length", "Length", GH_ParamAccess.item);
        p.AddNumberParameter("Angle", "Angle", "Radians", GH_ParamAccess.item);
        p.AddVectorParameter("Offset", "Offset", "Offset", GH_ParamAccess.item);
        p.AddTextParameter("Groups", "Groups", "Groups", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var b=Item<GH_FrameElement>(da,0).Value; da.SetData(0,b.Id); da.SetData(1,new Line(b.NodeStart.Position,b.NodeEnd.Position)); da.SetData(2,new GH_FrameProperty(b.FrameProperty)); da.SetData(3,b.NodeStart.Position.DistanceTo(b.NodeEnd.Position)); da.SetData(4,b.Angle); da.SetData(5,b.Offset); da.SetDataList(6,b.Groups.Select(x=>x.GroupFullName));
    }
}
