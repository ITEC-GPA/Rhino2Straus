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

public sealed class BeamOffsetComponent : SafeComponent
{
    public BeamOffsetComponent() : base("Beam Offset and Orientation", "Beam Offset and Orientation", "Beam Offset and Orientation", ComponentTopics.Tab, ComponentTopics.AxesOffsets) { }
    public override Guid ComponentGuid => new Guid("9a95f5ee-81a4-579c-931f-92ef81279dfc");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
        p.AddVectorParameter("Offset", "Offset", "Principal 1,2 offsets; Z must be zero", GH_ParamAccess.item);
        p.AddNumberParameter("Angle", "Angle", "Principal reference angle in degrees", GH_ParamAccess.item, 0.0);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var b=new FrameElementModel(Item<GH_FrameElement>(da,0).Value);var o=Item<Vector3d>(da,1);double a=Item<double>(da,2);
        if(!o.IsValid||Math.Abs(o.Z)>1e-12||!double.IsFinite(a))throw new ArgumentException("Use finite principal 1,2 offsets with Z=0.");b.Offset=o;b.Angle=RhinoMath.ToRadians(a);b.Breps=new();da.SetData(0,new GH_FrameElement(b));
    }
}
