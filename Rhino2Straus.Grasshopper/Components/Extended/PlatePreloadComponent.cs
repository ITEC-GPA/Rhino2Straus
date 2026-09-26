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

public sealed class PlatePreloadComponent : SafeComponent
{
    public PlatePreloadComponent() : base("Plate Preload", "Plate Preload", "Plate Preload", "Rhino2Straus", "06-Loads") { }
    public override Guid ComponentGuid => new Guid("91b858a9-5fe4-5bd6-b131-942b90582b49");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);
        p.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
        p.AddVectorParameter("Values", "Values", "Local XX, YY, XY stress (kPa) or dimensionless strain", GH_ParamAccess.item, Vector3d.Zero);
        p.AddBooleanParameter("Strain", "Strain", "Strain", GH_ParamAccess.item, false);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var a=new AreaElementModel(Item<GH_AreaElement>(da,0).Value); a.AreaLoadList.Add(new AreaPreloadModel { LoadCase=Item<GH_LoadCase>(da,1).Value,Value=Item<Vector3d>(da,2),Strain=Item<bool>(da,3) }); da.SetData(0,new GH_AreaElement(a));
    }
}
