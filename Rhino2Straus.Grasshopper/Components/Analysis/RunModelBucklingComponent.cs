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

public sealed class RunModelBucklingComponent : AsyncSolverComponent<ModelModel>
{
    public RunModelBucklingComponent() : base("Run Model Linear Buckling", "Run Model Linear Buckling", "Buckling using verified initial static results from the same Model; modal displacements are normalized mode shapes.", ComponentTopics.Tab, ComponentTopics.Analysis) { }
    public override Guid ComponentGuid => new Guid("323cda05-f8cf-506e-bba6-af56a2d89dde");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        p.AddTextParameter("Model path", "Model path", "Model path", GH_ParamAccess.item);
        p.AddBooleanParameter("Run", "Run", "Button or false-to-true transition", GH_ParamAccess.item, false);
        p.AddBooleanParameter("Overwrite", "Overwrite", "Overwrite", GH_ParamAccess.item, false);
        p.AddTextParameter("Initial LSA", "Initial LSA", "Verified LSA from this Model", GH_ParamAccess.item);
        p.AddIntegerParameter("Variable result case", "Variable result case", "Variable result case", GH_ParamAccess.item, 1);
        p.AddIntegerParameter("Fixed result case", "Fixed result case", "0 means none", GH_ParamAccess.item, 0);
        p.AddIntegerParameter("Modes", "Modes", "Modes", GH_ParamAccess.item, 10);
        p.AddNumberParameter("Shift", "Shift", "Shift", GH_ParamAccess.item, 0.0);
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
        string initial = Item<string>(da,4);
        int variable = Item<int>(da,5);
        int fixedCase = Item<int>(da,6);
        int modes = Item<int>(da,7);
        double shift = Item<double>(da,8);
        string key = ModelFingerprint.Compute(input) + System.Text.Json.JsonSerializer.Serialize(new object[] { path, initial, variable, fixedCase, modes, shift });
        var operation = Operation(da);
        var solved = operation.UpdatePreparedAsync(key, run,
            () =>
            {
                var snapshot = new ModelModel(input); snapshot.ClearResults();
                var initialFile = AnalysisFileState.Capture(initial);
                var initialModel = AnalysisFileState.Capture(System.IO.Path.ChangeExtension(initial, ".st7"));
                var manifest = AnalysisFileState.Capture(ResultProvenance.ManifestPath(initial));
                return cancellation =>
                {
                    initialFile.RequireUnchanged(); initialModel.RequireUnchanged(); manifest.RequireUnchanged();
                    return ModelAnalysisService.RunBucklingAsync(snapshot,path,overwrite,initial, variable, fixedCase, modes, shift, cancellation: cancellation);
                };
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
