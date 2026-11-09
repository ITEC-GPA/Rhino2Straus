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

public sealed class RunNonlinearStaticComponent : AsyncSolverComponent<string>
{
    public RunNonlinearStaticComponent() : base("Run Nonlinear Static", "Run Nonlinear Static", "Replaces the unstaged nonlinear increment table with a ramp of one load case. Freedom case 1 is held at factor 1; existing material laws are used", ComponentTopics.Tab, ComponentTopics.Analysis) { }
    public override Guid ComponentGuid => new Guid("410900f3-b729-5c92-9ac8-68f2241a1a07");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Model path", "Model path", "Model path", GH_ParamAccess.item);
        p.AddBooleanParameter("Run", "Run", "False-to-true transition or button", GH_ParamAccess.item, false);
        p.AddBooleanParameter("Overwrite results", "Overwrite results", "Overwrite results", GH_ParamAccess.item, false);
        p.AddIntegerParameter("Load case", "Load case", "Load case", GH_ParamAccess.item, 1);
        p.AddIntegerParameter("Steps", "Steps", "Steps", GH_ParamAccess.item, 10);
        p.AddNumberParameter("Final factor", "Final factor", "Final factor", GH_ParamAccess.item, 1.0);
        p.AddBooleanParameter("Geometric nonlinear", "Geometric nonlinear", "Geometric nonlinear", GH_ParamAccess.item, true);
        p.AddBooleanParameter("Material nonlinear", "Material nonlinear", "Material nonlinear", GH_ParamAccess.item, false);
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
        int loadCase = Item<int>(da,3);
        int steps = Item<int>(da,4);
        double factor = Item<double>(da,5);
        bool geometry = Item<bool>(da,6);
        bool material = Item<bool>(da,7);
        string key = System.Text.Json.JsonSerializer.Serialize(new object[] { path, loadCase, steps, factor, geometry, material });
        var operation = Operation(da);
        string result = operation.UpdatePreparedAsync(key, run,
            () =>
            {
                var source = AnalysisFileState.Capture(path);
                return cancellation =>
                {
                    source.RequireUnchanged();
                    return StrausFileService.RunNonlinearStaticAsync(path, overwrite, loadCase, steps, factor, geometry, material, cancellation: cancellation);
                };
            }, _ => "Completed");
        da.SetData(0,result ?? "");da.SetData(1,operation.Status);Report(operation);
    }
}
