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

public sealed class PlateOffsetComponent : SafeComponent
{
    public PlateOffsetComponent() : base("Plate Offset and Orientation", "Plate Offset and Orientation", "Plate Offset and Orientation", "Rhino2Straus", "04-Attributes") { }
    public override Guid ComponentGuid => new Guid("1bfc9052-c962-57e8-a6a9-ddf02109a1e8");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);
        p.AddNumberParameter("Offset", "Offset", "Signed local normal offset; added to property offset", GH_ParamAccess.item);
        p.AddNumberParameter("Angle", "Angle", "Local in-plane angle in degrees", GH_ParamAccess.item, 0.0);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var p=new AreaElementModel(Item<GH_AreaElement>(da,0).Value);p.Offset=Item<double>(da,1);p.Angle=RhinoMath.ToRadians(Item<double>(da,2));p.Brep=null;da.SetData(0,new GH_AreaElement(p));
    }
}
