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

public sealed class PlateEdgePressureComponent : SafeComponent
{
    public PlateEdgePressureComponent() : base("Plate Edge Pressure", "Plate Edge Pressure", "Plate Edge Pressure", "Rhino2Straus", "06-Loads") { }
    public override Guid ComponentGuid => new Guid("b1d1120f-e6da-55b6-93de-a828586c8be3");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);
        p.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
        p.AddIntegerParameter("Edge", "Edge", "Edge number, starting at 1", GH_ParamAccess.item);
        p.AddNumberParameter("Pressure", "Pressure", "Normal pressure applied to the plate edge", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var a=new AreaElementModel(Item<GH_AreaElement>(da,0).Value); int edge=Item<int>(da,2);
        if(edge<1 || edge>a.NodeList.Count) throw new ArgumentException("Edge is outside the plate.");
        a.AreaLoadList.Add(new AreaEdgePressureModel { LoadCase=Item<GH_LoadCase>(da,1).Value,Edge=edge,Value=Item<double>(da,3) }); da.SetData(0,new GH_AreaElement(a));
    }
}
