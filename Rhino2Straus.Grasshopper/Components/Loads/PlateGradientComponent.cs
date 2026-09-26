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

public sealed class PlateGradientComponent : SafeComponent
{
    public PlateGradientComponent() : base("Plate Temperature Gradient", "Plate Temperature Gradient", "Plate Temperature Gradient", ComponentTopics.Tab, ComponentTopics.Loads) { }
    public override Guid ComponentGuid => new Guid("26ddc3f2-abc4-5f63-962d-fa612cfaf6b5");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);
        p.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
        p.AddNumberParameter("Gradient", "Gradient", "Temperature difference per model length through thickness", GH_ParamAccess.item, 0.0);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var a=new AreaElementModel(Item<GH_AreaElement>(da,0).Value); a.AreaLoadList.Add(new AreaTemperatureGradientModel { LoadCase=Item<GH_LoadCase>(da,1).Value,Gradient=Item<double>(da,2) }); da.SetData(0,new GH_AreaElement(a));
    }
}
