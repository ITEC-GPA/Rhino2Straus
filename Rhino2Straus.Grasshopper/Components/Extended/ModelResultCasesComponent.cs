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

public sealed class ModelResultCasesComponent : SafeComponent
{
    public ModelResultCasesComponent() : base("Model Result Cases", "Model Result Cases", "Model Result Cases", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("11b2087e-d038-52db-9a57-3a662d981647");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddIntegerParameter("Numbers", "Numbers", "Numbers", GH_ParamAccess.list);
        p.AddTextParameter("Names", "Names", "Names", GH_ParamAccess.list);
        p.AddBooleanParameter("Secondary", "Secondary", "API secondary result case; use the returned names to identify combinations", GH_ParamAccess.list);
        p.AddBooleanParameter("Complete", "Complete", "Complete", GH_ParamAccess.item);
        p.AddBooleanParameter("Provenance verified", "Provenance verified", "Provenance verified", GH_ParamAccess.item);
        p.AddTextParameter("Unavailable", "Unavailable", "Unavailable", GH_ParamAccess.list);
        p.AddGenericParameter("Results", "Results", "Results", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var m=Item<GH_Model>(da,0).Value;var r=m.Results??throw new InvalidOperationException("Model has no results.");r.ValidateFor(m);
        da.SetDataList(0,r.Cases.Select(x=>x.Number));da.SetDataList(1,r.Cases.Select(x=>x.Name));da.SetDataList(2,r.Cases.Select(x=>x.Secondary));
        da.SetData(3,r.IsComplete);da.SetData(4,r.ProvenanceVerified);da.SetDataList(5,r.Issues.Select(x=>x.ToString()));da.SetData(6,new GH_ObjectWrapper(r));
    }
}
