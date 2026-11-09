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

public sealed class RunModelModalComponent : AsyncSolverComponent<ModelModel>
{
    public RunModelModalComponent() : base("Run Model Natural Frequencies", "Run Model Natural Frequencies", "Modal amplitudes follow Straus7 normalization; they are not displacements under a physical load case.", ComponentTopics.Tab, ComponentTopics.Analysis) { }
    public override Guid ComponentGuid => new Guid("7fdd9a12-3599-58dc-a1e5-7a49d58bcd53");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        p.AddTextParameter("Model path", "Model path", "Model path", GH_ParamAccess.item);
        p.AddBooleanParameter("Run", "Run", "Button or false-to-true transition", GH_ParamAccess.item, false);
        p.AddBooleanParameter("Overwrite", "Overwrite", "Overwrite", GH_ParamAccess.item, false);
        p.AddIntegerParameter("Modes", "Modes", "Modes", GH_ParamAccess.item, 10);
        p.AddNumberParameter("Shift Hz", "Shift Hz", "Shift Hz", GH_ParamAccess.item, 0.0);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        p.AddTextParameter("Result path", "Result path", "Result path", GH_ParamAccess.item);
        p.AddTextParameter("Status", "Status", "Status", GH_ParamAccess.item);
        p.AddTextParameter("Unavailable", "Unavailable", "Unavailable", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var input = Item<GH_Model>(da,0).Value;
        string path = System.IO.Path.GetFullPath(Item<string>(da,1));
        bool run = Item<bool>(da,2), overwrite = Item<bool>(da,3);
        int modes = Item<int>(da,4);
        double shift = Item<double>(da,5);
        string key = ModelFingerprint.Compute(input) + System.Text.Json.JsonSerializer.Serialize(new object[] { path, modes, shift });
        var operation = Operation(da);
        var solved = operation.UpdatePreparedAsync(key, run,
            () =>
            {
                var snapshot = new ModelModel(input); snapshot.ClearResults();
                return cancellation => ModelAnalysisService.RunModalAsync(snapshot,path,overwrite,modes, shift, cancellation: cancellation);
            },
            value => value.Results.IsComplete ? "Completed" : "Completed with unavailable quantities");
        // Never pass old incoming results through while the requested analysis is pending/obsolete.
        var fallback = input.Results == null ? input : new ModelModel(input);
        if (!ReferenceEquals(fallback,input)) fallback.ClearResults();
        da.SetData(0,new GH_Model(solved ?? fallback));da.SetData(1,solved?.Results.ResultPath ?? "");
        da.SetData(2,operation.Status);da.SetDataList(3,solved?.Results.Issues.Select(x=>x.ToString()) ?? Array.Empty<string>());
        Report(operation);
        if(solved?.Results.Issues.Count>0)AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,$"{solved.Results.Issues.Count} result quantities unavailable; inspect Unavailable.");
    }
}
