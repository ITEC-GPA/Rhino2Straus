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

public sealed class NodeTemperatureComponent : SafeComponent
{
    public NodeTemperatureComponent() : base("Node Temperature", "Node Temperature", "Node Temperature", ComponentTopics.Tab, ComponentTopics.Loads) { }
    public override Guid ComponentGuid => new Guid("90f7fcd1-e989-5bfa-9174-a1550a179dbe");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
        p.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
        p.AddNumberParameter("Temperature", "Temperature", "Temperature in model C/F units", GH_ParamAccess.item, 0.0);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var n=new NodeElementModel(Item<GH_NodeElement>(da,0).Value); n.Temperatures.Add(new NodeTemperatureModel { LoadCase=Item<GH_LoadCase>(da,1).Value,Temperature=Item<double>(da,2) }); da.SetData(0,new GH_NodeElement(n));
    }
}
