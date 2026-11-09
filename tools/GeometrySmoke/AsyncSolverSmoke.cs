using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2Straus.Api;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Loads;
using Rhino2Straus.Core.Models;
using Rhino2Straus.Grasshopper.Datatype;
using Rhino2Straus.Import;

internal static class AsyncSolverSmoke
{
    private static void PumpUntil(Func<bool> condition, int seconds = 15)
    {
        var timer = Stopwatch.StartNew();
        while (!condition())
        {
            if (timer.Elapsed.TotalSeconds > seconds) throw new TimeoutException("Grasshopper async completion timed out.");
            Application.DoEvents();
            Thread.Sleep(5);
        }
    }

    private static Dictionary<int, AsyncTriggeredOperation<T>> Operations<T>(GH_Component component) where T : class =>
        (Dictionary<int, AsyncTriggeredOperation<T>>)component.GetType().BaseType
            .GetField("operations", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(component);

    private static void Set(GH_Component component, int index, object value)
    {
        var parameter = component.Params.Input[index];
        switch (parameter)
        {
            case Param_String text: text.PersistentData.Clear(); text.PersistentData.Append(new GH_String((string)value)); break;
            case Param_Boolean boolean: boolean.PersistentData.Clear(); boolean.PersistentData.Append(new GH_Boolean((bool)value)); break;
            case Param_Integer integer: integer.PersistentData.Clear(); integer.PersistentData.Append(new GH_Integer((int)value)); break;
            case Param_GenericObject generic: generic.PersistentData.Clear(); generic.PersistentData.Append(new GH_Model((ModelModel)value)); break;
            default: throw new ArgumentException("Unsupported fixture parameter.");
        }
        parameter.ExpireSolution(false);
        component.ExpireSolution(false);
    }

    private static object Output(GH_Component component, int index) =>
        component.Params.Output[index].VolatileData.AllData(true).FirstOrDefault()?.ScriptVariable();

    private static GH_Component Add(Assembly assembly, GH_Document document, string name)
    {
        var component = (GH_Component)Activator.CreateInstance(assembly.GetTypes().Single(t => t.Name == name));
        document.AddObject(component, false);
        return component;
    }

    public static void Run(Assembly assembly, Action<bool, string> check, bool native)
    {
        GH_Document.EnableSolutions = true;
        using var document = new GH_Document { Enabled = true };
        Grasshopper.Instances.DocumentServer.AddDocument(document);
        try
        {
            var component = Add(assembly, document, "RunLinearStaticComponent");
            Set(component, 0, "test.st7");
            document.NewSolution(false);
            var states = Operations<string>(component);
            states[0].Dispose();
            using var entered = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            int calls = 0, beats = 0;
            using var heartbeat = new System.Windows.Forms.Timer { Interval = 10 };
            heartbeat.Tick += (_, _) => beats++;
            heartbeat.Start();
            var operation = new AsyncTriggeredOperation<string>((_, token) => Task.Run(() =>
            {
                Interlocked.Increment(ref calls); entered.Set();
                if (!release.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Fixture not released.");
                return "current.LSA";
            }));
            states[0] = operation;
            try
            {
                Set(component, 1, true);
                document.NewSolution(false);
                PumpUntil(() => entered.IsSet && beats >= 3);
                check(operation.IsPending && calls == 1, "Async GH returns to the message pump while work is pending");
                Set(component, 1, false); document.NewSolution(false);
                check(operation.IsPending, "Releasing the Run button does not cancel the job");
                release.Set();
                PumpUntil(() => (string)Output(component, 0) == "current.LSA");
                check(calls == 1 && operation.Status == "Completed", "Async completion schedules GH outputs without another Run");
                component.ComputeData();
                check(states.Count == 1, "Reading an already computed component retains its async state");

                Set(component, 0, "changed.st7"); document.NewSolution(false);
                check((string)Output(component, 0) == "" && operation.Value == null, "Changed GH input clears completed results");
                Set(component, 1, true); document.NewSolution(false);
                PumpUntil(() => (string)Output(component, 0) == "current.LSA");
                check(calls == 2, "New Run edge starts exactly one replacement job");

                // A deliberately non-cancellable old native job finishes after A -> B -> A.
                states[0].Dispose();
                using var lateEntered = new ManualResetEventSlim();
                using var lateRelease = new ManualResetEventSlim();
                using var lateFinished = new ManualResetEventSlim();
                var obsolete = new AsyncTriggeredOperation<string>((_, token) => Task.Run(() =>
                {
                    lateEntered.Set();
                    if (!lateRelease.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
                    lateFinished.Set(); return "obsolete.LSA";
                }));
                states[0] = obsolete;
                try
                {
                    Set(component, 0, "A.st7"); document.NewSolution(false);
                    PumpUntil(() => lateEntered.IsSet);
                    Set(component, 0, "B.st7"); document.NewSolution(false);
                    Set(component, 0, "A.st7"); document.NewSolution(false);
                    lateRelease.Set(); PumpUntil(() => lateFinished.IsSet);
                    Application.DoEvents(); document.NewSolution(false);
                    check(obsolete.Value == null && (string)Output(component, 0) == "", "GH A-B-A never publishes an obsolete completion");
                    document.RemoveObject(component, false);
                    check(obsolete.Status == "Disposed" && states.Count == 0, "Removing an async component releases all job owners");
                }
                finally { lateRelease.Set(); }
            }
            finally { release.Set(); }

            var closing = Add(assembly, document, "RunLinearStaticComponent");
            Set(closing, 0, "closing.st7"); document.NewSolution(false);
            var closeOperation = Operations<string>(closing)[0];
            closing.DocumentContextChanged(document, GH_DocumentContext.Close);
            check(closeOperation.Status == "Disposed", "Closing a document invalidates its async state");
            closing.DocumentContextChanged(document, GH_DocumentContext.Open);
            Set(closing, 0, "reopened.st7"); document.NewSolution(false);
            check(!ReferenceEquals(closeOperation, Operations<string>(closing)[0]), "Reopening obtains a fresh job lifetime");
            ((Param_String)closing.Params.Input[0]).PersistentData.Clear();
            closing.Params.Input[0].ExpireSolution(false); closing.ExpireSolution(false); document.NewSolution(false);
            check(Operations<string>(closing).Count == 0, "Missing required input retires pending async state");
            Set(closing, 1, true); document.NewSolution(false);
            Set(closing, 0, "restored.st7"); document.NewSolution(false);
            check(!Operations<string>(closing)[0].IsPending && (string)Output(closing, 1) == "Run required",
                "Restoring missing inputs cannot rearm a held Run");
            Set(closing, 1, false); document.NewSolution(false);
            Set(closing, 1, true); document.NewSolution(false);
            PumpUntil(() => (string)Output(closing, 1) == "Failed");
            check(closing.RuntimeMessages(GH_RuntimeMessageLevel.Error).Any(message => message.Contains("missing or changed")),
                "A fresh false-true edge after input restoration accepts a new job");
            closing.DocumentContextChanged(document, GH_DocumentContext.Lock);
            closing.DocumentContextChanged(document, GH_DocumentContext.Unlock);
            closing.ExpireSolution(false); document.NewSolution(false);
            check(!Operations<string>(closing)[0].IsPending && (string)Output(closing, 1) == "Run required",
                "Unlocking a document cannot rearm a held Run");
        }
        finally { Grasshopper.Instances.DocumentServer.RemoveDocument(document); }
        if (native) RunNative(assembly, check);
    }

    private static void RunNative(Assembly assembly, Action<bool, string> check)
    {
        string folder = Path.Combine(Path.GetTempPath(), "Rhino2Straus-async-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        Console.WriteLine("Async native files: " + folder);
        var lc = new LoadCaseModel("Load");
        var start = new NodeElementModel(0, 0, 0) { Support = new NodeSupportModel(true, true, true, true, true, true) };
        var tip = new NodeElementModel(2, 0, 0);
        tip.NodalLoadList.Add(new NodalLoadModel(lc, -1, 0, -1, 0, 0, 0));
        var section = new FrameSectionModel { Name = "Generic", SectionGeometryType = FrameSectionModel.SectionGeometryTypes.Generic,
            SectionArea = .01, I11 = .0001, I22 = .0001, J = .0002, ShearA1 = .01, ShearA2 = .01, Breps = new() };
        var beam = new FrameElementModel(start, tip, new FramePropertyModel("Beam", MaterialModel.S355, section), 0, Vector3d.Zero);
        var model = new ModelModel(); model.BuildModel(inputFrames: new() { beam });
        var initial = ModelAnalysisService.RunStatic(model, Path.Combine(folder, "initial.st7"), false);
        string[] names = { "RunLinearStaticComponent", "RunNaturalFrequencyComponent", "RunLinearBucklingComponent", "RunNonlinearStaticComponent",
            "RunModelCasesComponent", "RunModelModalComponent", "RunModelBucklingComponent", "RunModelNonlinearComponent" };
        foreach (string name in names)
        {
            using var document = new GH_Document { Enabled = true };
            Grasshopper.Instances.DocumentServer.AddDocument(document);
            try
            {
                bool integrated = name.StartsWith("RunModel", StringComparison.Ordinal);
                int runIndex = integrated ? 2 : 1, pathIndex = integrated ? 1 : 0;
                var component = Add(assembly, document, name);
                string path = Path.Combine(folder, name + ".st7");
                if (integrated) Set(component, 0, model); else File.Copy(initial.Results.ModelPath, path);
                Set(component, pathIndex, path);
                if (name == "RunNaturalFrequencyComponent") Set(component, 3, 1);
                if (name == "RunLinearBucklingComponent") { Set(component, 3, 1); Set(component, 4, initial.Results.ResultPath); }
                if (name == "RunModelModalComponent") Set(component, 4, 1);
                if (name == "RunModelBucklingComponent") { Set(component, 4, initial.Results.ResultPath); Set(component, 7, 1); }
                if (name == "RunModelNonlinearComponent") Set(component, 4, "Load");
                document.NewSolution(false);
                int beats = 0;
                using var heartbeat = new System.Windows.Forms.Timer { Interval = 10 };
                heartbeat.Tick += (_, _) => beats++; heartbeat.Start();
                Set(component, runIndex, true);
                var watch = Stopwatch.StartNew(); document.NewSolution(false); watch.Stop();
                check(watch.ElapsedMilliseconds < 1500, name + " starts without waiting for its native solver");
                Set(component, runIndex, false); document.NewSolution(false);
                int statusIndex = integrated ? 2 : 1;
                PumpUntil(() =>
                {
                    string status = Output(component, statusIndex) as string;
                    if (status == "Failed") throw new Exception(name + ": " + string.Join("; ", component.RuntimeMessages(GH_RuntimeMessageLevel.Error)));
                    return status?.StartsWith("Completed", StringComparison.Ordinal) == true;
                }, 90);
                string results = Output(component, integrated ? 1 : 0) as string;
                check(beats >= 3 && File.Exists(results), name + " completes with UI heartbeats and native results");
                if (integrated)
                {
                    var solved = (ModelModel)Output(component, 0);
                    solved.Results.ValidateFor(solved, requireProvenance: true);
                    check(ModelFingerprint.Compute(model) == ModelFingerprint.Compute(solved), name + " publishes the matching Model snapshot");
                }
            }
            finally { Grasshopper.Instances.DocumentServer.RemoveDocument(document); }
        }
        using var staleDocument = new GH_Document { Enabled = true };
        Grasshopper.Instances.DocumentServer.AddDocument(staleDocument);
        try
        {
            var component = Add(assembly, staleDocument, "RunModelCasesComponent");
            Set(component, 0, initial); Set(component, 1, Path.Combine(folder, "obsolete.st7"));
            staleDocument.NewSolution(false);
            check(((ModelModel)Output(component, 0)).Results == null, "Async Model output clears incoming results before a new analysis");
            Set(component, 2, true); staleDocument.NewSolution(false);
            PumpUntil(() => Operations<ModelModel>(component)[0].Status == "Running", 90);
            bool busy = false;
            try { StrausRuntime.Configure(); } catch (StrausBusyException) { busy = true; }
            check(busy, "An unrelated native UI request fails fast while the real solver is active");
            var changed = new ModelModel(model); changed.FrameElements.Values.First().Angle += .25;
            Set(component, 0, changed); staleDocument.NewSolution(false);
            check(((ModelModel)Output(component, 0)).Results == null && (string)Output(component, 1) == "",
                "Changed Model clears pending results without restarting a held Run");
            // Wait for the cancelled generation's solver to exit; never terminate its native process.
            PumpUntil(() => { try { StrausRuntime.Configure(); return true; } catch (StrausBusyException) { return false; } }, 90);
            Application.DoEvents(); staleDocument.NewSolution(false);
            check((string)Output(component, 2) == "Run required" && (string)Output(component, 1) == "",
                "A real obsolete completion is discarded and releases the native owner");
            check(File.Exists(Path.Combine(folder, "obsolete.LSA")), "Discarding obsolete output leaves the completed solver file available on disk");
            Set(component, 1, Path.Combine(folder, "after-obsolete.st7")); Set(component, 2, false); staleDocument.NewSolution(false);
            Set(component, 2, true); staleDocument.NewSolution(false);
            PumpUntil(() => ((string)Output(component, 2))?.StartsWith("Completed", StringComparison.Ordinal) == true, 90);
            var solved = (ModelModel)Output(component, 0);
            check(ModelFingerprint.Compute(changed) == ModelFingerprint.Compute(solved),
                "A fresh Run after obsolete work publishes results for the changed Model");
        }
        finally { Grasshopper.Instances.DocumentServer.RemoveDocument(staleDocument); }

    }
}
