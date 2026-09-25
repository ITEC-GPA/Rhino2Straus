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

public sealed class BeamAxesComponent : SafeComponent
{
    public BeamAxesComponent() : base("Beam Local Axes", "Beam Local Axes", "Beam Local Axes", "Rhino2Straus", "09-Tools") { }
    public override Guid ComponentGuid => new Guid("426c4ddb-78f2-585a-8061-a4a3df13cbcb");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Element", "Element", "Element", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddVectorParameter("Axis 1", "Axis 1", "Axis 1", GH_ParamAccess.item);
        p.AddVectorParameter("Axis 2", "Axis 2", "Axis 2", GH_ParamAccess.item);
        p.AddVectorParameter("Axis 3", "Axis 3", "Axis 3", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var a=Item<GH_FrameElement>(da,0).Value; a.GetLocalAxes(true,out var z,out var y,out var x,out var transform); da.SetData(0,x); da.SetData(1,y); da.SetData(2,z);
    }
}
