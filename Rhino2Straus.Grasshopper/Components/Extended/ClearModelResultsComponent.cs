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

public sealed class ClearModelResultsComponent : SafeComponent
{
    public ClearModelResultsComponent() : base("Clear Model Results", "Clear Model Results", "Clear Model Results", "Rhino2Straus", "07-Model") { }
    public override Guid ComponentGuid => new Guid("a1bf7fee-60e3-5015-80a8-edaa147b0d24");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var model=new ModelModel(Item<GH_Model>(da,0).Value);model.ClearResults();da.SetData(0,new GH_Model(model));
    }
}
