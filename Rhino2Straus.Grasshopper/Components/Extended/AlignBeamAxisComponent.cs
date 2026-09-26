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

public sealed class AlignBeamAxisComponent : SafeComponent
{
    public AlignBeamAxisComponent() : base("Align Beam Local Axis", "Align Beam Local Axis", "Align a local axis to a projected direction. Local loads and offsets follow the rotated axes. Input stays unchanged.", "Rhino2Straus", "04-Geometry") { }
    public override Guid ComponentGuid => new Guid("ae20ccaf-f1e3-599e-bcc2-18e479bcb81f");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
        p.AddVectorParameter("Direction", "Direction", "Desired direction projected into the transverse beam plane / plate plane", GH_ParamAccess.item, Vector3d.ZAxis);
        p.AddIntegerParameter("Axis", "Axis", "Axis 1 or 2", GH_ParamAccess.item, 1);
        p.AddBooleanParameter("Principal", "Principal", "True: principal section axes; false: geometric section axes", GH_ParamAccess.item, true);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
        p.AddPlaneParameter("Axes", "Axes", "Axes", GH_ParamAccess.item);
        p.AddNumberParameter("Angle", "Angle", "Final orientation in radians", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var element=ElementAlignment.Beam(Item<GH_FrameElement>(da,0).Value,Item<Vector3d>(da,1),Item<int>(da,2),Item<bool>(da,3));
        element.GetLocalAxes(Item<bool>(da,3),out var z,out var y,out var x,out _); var origin=element.NodeStart.Position;
        da.SetData(0,new GH_FrameElement(element));da.SetData(1,new Plane(origin,x,y));da.SetData(2,element.Angle);
    }
}
