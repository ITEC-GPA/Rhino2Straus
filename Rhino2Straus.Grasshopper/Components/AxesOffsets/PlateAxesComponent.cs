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

public sealed class PlateAxesComponent : SafeComponent
{
    public PlateAxesComponent() : base("Plate Local Axes", "Plate Local Axes", "Plate Local Axes", ComponentTopics.Tab, ComponentTopics.AxesOffsets) { }
    public override Guid ComponentGuid => new Guid("d36c6a2f-6ebd-5458-af04-e4bd82902d89");
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

        var a=Item<GH_AreaElement>(da,0).Value; a.GetLocalAxes(out var x,out var y,out var z); da.SetData(0,x); da.SetData(1,y); da.SetData(2,z);
    }
}
