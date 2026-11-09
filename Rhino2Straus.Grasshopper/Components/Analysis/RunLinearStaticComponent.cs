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

public sealed class RunLinearStaticComponent : AsyncSolverComponent<string>
{
    public RunLinearStaticComponent() : base("Run Linear Static", "Run Linear Static", "Run Linear Static", ComponentTopics.Tab, ComponentTopics.Analysis) { }
    public override Guid ComponentGuid => new Guid("d210a91e-8e0e-5b67-ac0c-68ca3a8e2aa4");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Model path", "Model path", "Model path", GH_ParamAccess.item);
        p.AddBooleanParameter("Run", "Run", "Use a button or a false-to-true transition", GH_ParamAccess.item, false);
        p.AddBooleanParameter("Overwrite results", "Overwrite results", "Overwrite results", GH_ParamAccess.item, false);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddTextParameter("Result path", "Result path", "Result path", GH_ParamAccess.item);
        p.AddTextParameter("Status", "Status", "Status", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        string path = System.IO.Path.GetFullPath(Item<string>(da,0));
        bool run = Item<bool>(da,1), overwrite = Item<bool>(da,2);

        string key = System.Text.Json.JsonSerializer.Serialize(new object[] { path });
        var operation = Operation(da);
        string result = operation.UpdatePreparedAsync(key, run,
            () =>
            {
                var source = AnalysisFileState.Capture(path);
                return cancellation =>
                {
                    source.RequireUnchanged();
                    return StrausFileService.RunLinearStaticAsync(path, overwrite, cancellation: cancellation);
                };
            }, _ => "Completed");
        da.SetData(0,result ?? "");da.SetData(1,operation.Status);Report(operation);
    }
}
