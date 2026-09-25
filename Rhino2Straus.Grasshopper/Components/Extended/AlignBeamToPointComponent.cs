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

public sealed class AlignBeamToPointComponent : SafeComponent
{
    public AlignBeamToPointComponent() : base("Align Beam Axis To Point", "Align Beam Axis To Point", "Align Beam Axis To Point", "Rhino2Straus", "04-Geometry") { }
    public override Guid ComponentGuid => new Guid("2a57a7de-dd8b-5a04-a77f-d1b3e1ca318e");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
        p.AddPointParameter("Target", "Target", "Target", GH_ParamAccess.item);
        p.AddIntegerParameter("Axis", "Axis", "Axis", GH_ParamAccess.item, 1);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
        p.AddPlaneParameter("Axes", "Axes", "Axes", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var beam=Item<GH_FrameElement>(da,0).Value;var middle=beam.NodeStart.Position+(beam.NodeEnd.Position-beam.NodeStart.Position)*.5;
        var aligned=ElementAlignment.Beam(beam,Item<Point3d>(da,1)-middle,Item<int>(da,2));
        aligned.GetLocalAxes(true,out _,out var y,out var x,out _);da.SetData(0,new GH_FrameElement(aligned));da.SetData(1,new Plane(middle,x,y));
    }
}
