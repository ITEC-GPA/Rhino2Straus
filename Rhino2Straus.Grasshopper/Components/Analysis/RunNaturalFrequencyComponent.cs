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

public sealed class RunNaturalFrequencyComponent : AsyncSolverComponent<string>
{
    public RunNaturalFrequencyComponent() : base("Run Natural Frequency", "Run Natural Frequency", "Run natural frequency analysis and calculate mass participation", ComponentTopics.Tab, ComponentTopics.Analysis) { }
    public override Guid ComponentGuid => new Guid("69592316-e47a-57a8-b85d-09bc68563540");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Model path", "Model path", "Model path", GH_ParamAccess.item);
        p.AddBooleanParameter("Run", "Run", "False-to-true transition or button", GH_ParamAccess.item, false);
        p.AddBooleanParameter("Overwrite results", "Overwrite results", "Overwrite results", GH_ParamAccess.item, false);
        p.AddIntegerParameter("Modes", "Modes", "Modes", GH_ParamAccess.item, 10);
        p.AddNumberParameter("Shift Hz", "Shift Hz", "Shift Hz", GH_ParamAccess.item, 0.0);
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
        int modes = Item<int>(da,3);
        double shift = Item<double>(da,4);
        string key = System.Text.Json.JsonSerializer.Serialize(new object[] { path, modes, shift });
        var operation = Operation(da);
        string result = operation.UpdatePreparedAsync(key, run,
            () =>
            {
                var source = AnalysisFileState.Capture(path);
                return cancellation =>
                {
                    source.RequireUnchanged();
                    return StrausFileService.RunNaturalFrequencyAsync(path, overwrite, modes, shift, cancellation: cancellation);
                };
            }, _ => "Completed");
        da.SetData(0,result ?? "");da.SetData(1,operation.Status);Report(operation);
    }
}
