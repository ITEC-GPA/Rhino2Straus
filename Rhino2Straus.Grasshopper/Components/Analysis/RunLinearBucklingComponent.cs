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

public sealed class RunLinearBucklingComponent : AsyncSolverComponent<string>
{
    public RunLinearBucklingComponent() : base("Run Linear Buckling", "Run Linear Buckling", "Run eigenvalue buckling using an existing static stress state", ComponentTopics.Tab, ComponentTopics.Analysis) { }
    public override Guid ComponentGuid => new Guid("913420ec-f794-5996-b036-c5608b34c6ff");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Model path", "Model path", "Model path", GH_ParamAccess.item);
        p.AddBooleanParameter("Run", "Run", "False-to-true transition or button", GH_ParamAccess.item, false);
        p.AddBooleanParameter("Overwrite results", "Overwrite results", "Overwrite results", GH_ParamAccess.item, false);
        p.AddIntegerParameter("Modes", "Modes", "Modes", GH_ParamAccess.item, 10);
        p.AddTextParameter("Initial results", "Initial results", "Initial results", GH_ParamAccess.item);
        p.AddIntegerParameter("Variable case", "Variable case", "Variable case", GH_ParamAccess.item, 1);
        p.AddIntegerParameter("Fixed case", "Fixed case", "Fixed case", GH_ParamAccess.item, 0);
        p.AddNumberParameter("Shift", "Shift", "Shift", GH_ParamAccess.item, 0.0);
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
        string initial = Item<string>(da,4);
        int variable = Item<int>(da,5);
        int fixedCase = Item<int>(da,6);
        double shift = Item<double>(da,7);
        string key = System.Text.Json.JsonSerializer.Serialize(new object[] { path, modes, initial, variable, fixedCase, shift });
        var operation = Operation(da);
        string result = operation.UpdatePreparedAsync(key, run,
            () =>
            {
                var source = AnalysisFileState.Capture(path);
                var initialSource = AnalysisFileState.Capture(initial);
                return cancellation =>
                {
                    source.RequireUnchanged();
                    initialSource.RequireUnchanged();
                    return StrausFileService.RunLinearBucklingAsync(path, overwrite, modes, initial, variable, fixedCase, shift, cancellation: cancellation);
                };
            }, _ => "Completed");
        da.SetData(0,result ?? "");da.SetData(1,operation.Status);Report(operation);
    }
}
