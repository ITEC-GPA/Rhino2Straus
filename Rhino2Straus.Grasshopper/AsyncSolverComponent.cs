using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Windows.Forms.Timer;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino2Straus.Api;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Import;

namespace Rhino2Straus.Grasshopper;

/// <summary>The UI dispatcher owns native calls; the external solver runs asynchronously. No job captures a document or DA.</summary>
public abstract class AsyncSolverComponent<T> : SafeComponent where T : class
{
    private readonly Dictionary<int, AsyncTriggeredOperation<T>> operations = new();
    private readonly Dictionary<int, long> observed = new();
    private readonly HashSet<int> visited = new();
    private readonly HashSet<int> retiredSlots = new();
    private string waitDiagnostic;
    private Timer poller;
    private long filesRevision;
    private long lastFileCheck;

    protected AsyncSolverComponent(string name, string nickname, string description, string category, string subcategory)
        : base(name, nickname, description, category, subcategory) { }

    protected AsyncTriggeredOperation<T> Operation(IGH_DataAccess da)
    {
        int slot = da.Iteration;
        visited.Add(slot);
        if (!operations.TryGetValue(slot, out var operation))
        {
            // Capture only the UI dispatcher, never the component, document or data access.
            var context = new WindowsFormsSynchronizationContext();
            operations.Add(slot, operation = AsyncTriggeredOperation<T>.CreateAsync((work, token) => Execute(context, work, token)));
            if (retiredSlots.Remove(slot)) operation.RequireRunReset();
        }
        // Read before Update/output: a completion racing this solution must still trigger a refresh.
        if (operation.Value is { } cached && !AnalysisOutputFiles.IsCurrent(cached)) operation.Invalidate();
        observed[slot] = operation.Revision;
        return operation;
    }

    private static Task<T> Execute(SynchronizationContext context, Func<CancellationToken, Task<T>> work, CancellationToken token)
    {
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        context.Post(async _ =>
        {
            try
            {
                SynchronizationContext.SetSynchronizationContext(context);
                var result = await StrausRuntime.RunExclusiveAsync(async ct => AnalysisOutputFiles.Track(await work(ct)), token);
                completion.TrySetResult(result);
            }
            catch (OperationCanceledException) { completion.TrySetCanceled(token); }
            catch (Exception error) { completion.TrySetException(error); }
            finally { AnalysisOutputFiles.NotifyChanged(); }
        }, null);
        return completion.Task;
    }

    public override void ComputeData()
    {
        if (Phase == GH_SolutionPhase.Computed) { base.ComputeData(); return; }
        visited.Clear();
        try { base.ComputeData(); }
        finally
        {
            // Includes missing/invalid inputs: no solution branch means no live result owner.
            foreach (int slot in operations.Keys.Where(slot => !visited.Contains(slot)).ToArray())
            {
                retiredSlots.Add(slot);
                operations[slot].Dispose();
                operations.Remove(slot);
                observed.Remove(slot);
            }
        }
    }

    protected void Report(AsyncTriggeredOperation<T> operation)
    {
        Message = operation.Status;
        if (operation.IsPending && StrausRuntime.SolverWaitDiagnostic is { } diagnostic)
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, diagnostic);
        if (!string.IsNullOrEmpty(operation.Error)) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, operation.Error);
    }

    public override void AddedToDocument(GH_Document document)
    {
        base.AddedToDocument(document);
        StartPolling();
    }

    private void StartPolling()
    {
        if (poller != null) return;
        poller = new Timer { Interval = 100 };
        poller.Tick += Poll;
        poller.Start();
    }

    private void Poll(object sender, EventArgs args)
    {
        var document = OnPingDocument();
        if (document == null || Locked || !document.Enabled || !GH_Document.EnableSolutions) return;
        // Another solver may have overwritten the same native files. Invalidate old path outputs too.
        if (filesRevision != AnalysisOutputFiles.Revision || Environment.TickCount64 - lastFileCheck >= 1000)
        {
            filesRevision = AnalysisOutputFiles.Revision;
            lastFileCheck = Environment.TickCount64;
            foreach (var operation in operations.Values)
                if (operation.Value is { } value && !AnalysisOutputFiles.IsCurrent(value)) operation.Invalidate();
        }
        bool diagnosticChanged = waitDiagnostic != StrausRuntime.SolverWaitDiagnostic;
        waitDiagnostic = StrausRuntime.SolverWaitDiagnostic;
        if (!diagnosticChanged && !operations.Any(pair => !observed.TryGetValue(pair.Key, out long revision) || revision != pair.Value.Revision)) return;
        // WinForms timer already runs on the GH thread. Never recurse into an active solution.
        // ExpireSolution(true) also works in Rhino.Inside without a GH editor window.
        if (document.SolutionDepth == 0) ExpireSolution(true);
    }

    private void Stop()
    {
        if (poller != null) { poller.Stop(); poller.Tick -= Poll; poller.Dispose(); poller = null; }
        foreach (var pair in operations) { retiredSlots.Add(pair.Key); pair.Value.Dispose(); }
        operations.Clear(); observed.Clear(); visited.Clear();
    }

    public override void RemovedFromDocument(GH_Document document)
    {
        Stop();
        base.RemovedFromDocument(document);
    }

    public override void DocumentContextChanged(GH_Document document, GH_DocumentContext context)
    {
        if (context is GH_DocumentContext.Close or GH_DocumentContext.Unloaded or GH_DocumentContext.Lock) Stop();
        else if (context is GH_DocumentContext.Open or GH_DocumentContext.Loaded or GH_DocumentContext.Unlock) StartPolling();
        base.DocumentContextChanged(document, context);
    }
}
