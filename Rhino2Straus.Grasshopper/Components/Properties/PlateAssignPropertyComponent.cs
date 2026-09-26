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

public sealed class PlateAssignPropertyComponent : SafeComponent
{
    public PlateAssignPropertyComponent() : base("Assign Plate Property", "Assign Plate Property", "Assign Plate Property", ComponentTopics.Tab, ComponentTopics.Properties) { }
    public override Guid ComponentGuid => new Guid("79e2c5d0-60e2-5d1c-9b66-24c057f12411");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);
        p.AddGenericParameter("Property", "Property", "Property", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var p=new AreaElementModel(Item<GH_AreaElement>(da,0).Value);p.AreaProperty=Item<GH_AreaProperty>(da,1).Value;p.Material=p.AreaProperty.Material;p.AreaThickness=p.AreaProperty.Section;p.Brep=null;da.SetData(0,new GH_AreaElement(p));
    }
}
