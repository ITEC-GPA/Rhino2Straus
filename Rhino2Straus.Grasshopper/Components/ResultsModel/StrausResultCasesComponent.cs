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

public sealed class StrausResultCasesComponent : SafeComponent
{
    public StrausResultCasesComponent() : base("Straus Result Cases", "Straus Result Cases", "Straus Result Cases", ComponentTopics.Tab, ComponentTopics.ResultsModel) { }
    public override Guid ComponentGuid => new Guid("d0e739f8-93bd-5995-87cf-e2ede60eadc3");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Model path", "Model path", "Existing .st7 model path", GH_ParamAccess.item);
        p.AddTextParameter("Result path", "Result path", "Existing .LSA or compatible Straus7 result file", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddIntegerParameter("Numbers", "Numbers", "Numbers", GH_ParamAccess.list);
        p.AddTextParameter("Names", "Names", "Names", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var names=StrausFileService.ResultCases(Item<string>(da,0),Item<string>(da,1)); da.SetDataList(0,Enumerable.Range(1,names.Length)); da.SetDataList(1,names);
    }
}
