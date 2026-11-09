using System;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Rhino2Straus.Api;
using St7API;

namespace Rhino2Straus.Import;

public static partial class StrausFileService
{
    private static string RunAnalysis(string path, string extension, int solver, bool overwrite, Action<int> configure)
    {
        path = Path.GetFullPath(path);
        if (!File.Exists(path) || !path.EndsWith(".st7", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Supply an existing .st7 model.");
        string result = Path.ChangeExtension(path, extension);
        bool existed = File.Exists(result);
        if (existed && !overwrite) throw new IOException("Results already exist. Enable Overwrite results to run again.");
        DateTime previous = existed ? File.GetLastWriteTimeUtc(result) : DateTime.MinValue;
        using var session = new StrausSession(path, readOnly: false);
        configure(session.Id);
        session.Save();
        // Keep the solver's computation outside Rhino's process; all API calls still own the native gate.
        byte previousMode = 0;
        Check(St7.St7GetUseSolverDLL(ref previousMode), "Read solver execution mode");
        Check(St7.St7SetUseSolverDLL((byte)St7.btFalse), "Use external Straus7 solver");
        try { Check(St7.St7RunSolver(session.Id, solver, St7.smBackgroundRun, St7.btTrue), "Run Straus7 solver"); }
        finally { Check(St7.St7SetUseSolverDLL(previousMode), "Restore solver execution mode"); }
        RequireFreshResults(result, existed, previous);
        return result;
    }

    private static void RequireFreshResults(string result, bool existed, DateTime previous)
    {
        if (!File.Exists(result) || (existed && File.GetLastWriteTimeUtc(result) <= previous))
            throw new IOException("The solver did not produce fresh results. Inspect its log for convergence or licence errors.");
    }

    // The exclusive owner prevents UI reentrancy while the session's Monitor stays on the UI thread.
    // Do not use ConfigureAwait(false), Task.Run or a cancellation token on the process wait here.
    private static Task<string> RunAnalysisAsync(string path, string extension, int solver, bool overwrite,
        Action<int> configure, CancellationToken cancellation)
        => StrausRuntime.RunExclusiveAsync(async token =>
        {
            token.ThrowIfCancellationRequested();
            path = Path.GetFullPath(path);
            if (!File.Exists(path) || !path.EndsWith(".st7", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Supply an existing .st7 model.");
            string result = Path.ChangeExtension(path, extension);
            bool existed = File.Exists(result);
            if (existed && !overwrite) throw new IOException("Results already exist. Enable Overwrite results to run again.");
            DateTime previous = existed ? File.GetLastWriteTimeUtc(result) : DateTime.MinValue;
            using var session = new StrausSession(path, readOnly: false);
            configure(session.Id);
            token.ThrowIfCancellationRequested();
            session.Save();
            token.ThrowIfCancellationRequested();

            byte previousMode = 0;
            Check(St7.St7GetUseSolverDLL(ref previousMode), "Read solver execution mode");
            Check(St7.St7SetUseSolverDLL((byte)St7.btFalse), "Use external Straus7 solver");
            int processId = 0;
            Exception launchError = null, restoreError = null;
            try
            {
                Check(St7.St7RunSolverProcess(session.Id, solver, St7.smBackgroundRun, St7.btFalse, ref processId),
                    "Launch Straus7 solver");
            }
            catch (Exception error) { launchError = error; }
            finally
            {
                // Restore immediately after launch, but never close a live solver's session if restore fails.
                try { Check(St7.St7SetUseSolverDLL(previousMode), "Restore solver execution mode"); }
                catch (Exception error) { restoreError = error; }
            }

            if (processId > 0) await WaitForSolverExitAsync(processId);
            token.ThrowIfCancellationRequested();
            if (launchError != null) ExceptionDispatchInfo.Capture(launchError).Throw();
            if (restoreError != null) ExceptionDispatchInfo.Capture(restoreError).Throw();
            if (processId <= 0) throw new IOException("Straus7 did not return a valid solver process ID.");
            RequireFreshResults(result, existed, previous);
            return result;
        }, cancellation);

    private static async Task WaitForSolverExitAsync(int processId)
    {
        // This is the SDK's process identifier, not a Windows PID. Only the SDK can query it.
        while (true)
        {
            byte running = St7.btTrue;
            int code = St7.St7CheckSolverRunning(processId, ref running);
            if (code != 0)
            {
                StrausRuntime.ReportSolverWaitProblem($"Cannot determine solver state (Straus7 error {code}). The native API remains reserved; check the solver in Straus7. Waiting for confirmed termination; no process has been stopped.");
                await Task.Delay(1000);
                continue;
            }
            StrausRuntime.ReportSolverWaitProblem(null);
            if (running == St7.btFalse) return;
            // Cancellation discards the result, but must not release a still-running native session.
            await Task.Delay(100);
        }
    }

    public static Task<string> RunLinearStaticAsync(string path, bool overwriteResults, CancellationToken cancellation = default)
        => RunAnalysisAsync(path, ".LSA", St7.stLinearStatic, overwriteResults, _ => { }, cancellation);

    public static string RunNaturalFrequency(string path, bool overwrite, int modes, double shift)
        => RunAnalysis(path, ".NFA", St7.stNaturalFrequency, overwrite, NaturalFrequencyConfiguration(modes, shift));

    public static Task<string> RunNaturalFrequencyAsync(string path, bool overwrite, int modes, double shift,
        CancellationToken cancellation = default)
        => RunAnalysisAsync(path, ".NFA", St7.stNaturalFrequency, overwrite, NaturalFrequencyConfiguration(modes, shift), cancellation);

    private static Action<int> NaturalFrequencyConfiguration(int modes, double shift)
    {
        if (modes < 1 || !double.IsFinite(shift)) throw new ArgumentException("Use a positive mode count and finite shift in Hz.");
        return id => {
            Check(St7.St7SetNFANumModes(id, modes), "Set mode count");
            Check(St7.St7SetNFAShift(id, shift), "Set frequency shift");
            Check(St7.St7SetNFAModeParticipationCalculate(id, St7.btTrue), "Enable modal participation");
        };
    }

    public static string RunLinearBuckling(string path, bool overwrite, int modes, string initialResults, int variableCase, int fixedCase, double shift, AnalysisSelection selection = null)
        => RunAnalysis(path, ".LBA", St7.stLinearBuckling, overwrite,
            BucklingConfiguration(modes, initialResults, variableCase, fixedCase, shift, selection));

    public static Task<string> RunLinearBucklingAsync(string path, bool overwrite, int modes, string initialResults,
        int variableCase, int fixedCase, double shift, AnalysisSelection selection = null, CancellationToken cancellation = default)
        => RunAnalysisAsync(path, ".LBA", St7.stLinearBuckling, overwrite,
            BucklingConfiguration(modes, initialResults, variableCase, fixedCase, shift, selection), cancellation);

    private static Action<int> BucklingConfiguration(int modes, string initialResults, int variableCase, int fixedCase,
        double shift, AnalysisSelection selection)
    {
        if (modes < 1 || variableCase < 1 || fixedCase < 0 || !double.IsFinite(shift) || !File.Exists(initialResults)) throw new ArgumentException("Buckling needs initial static results, a positive variable case and mode count; fixed case 0 means none.");
        initialResults = Path.GetFullPath(initialResults);
        return id => {
            if (selection != null) ConfigureStaticSelection(id, selection);
            Check(St7.St7SetLBANumModes(id, modes), "Set buckling mode count");
            Check(St7.St7SetLBAShift(id, shift), "Set buckling shift");
            Check(St7.St7SetLBAInitial(id, Path.GetFullPath(initialResults), variableCase, fixedCase), "Set initial stress results");
        };
    }

    public static string RunNonlinearStatic(string path, bool overwrite, int loadCase, int steps, double factor, bool geometry, bool material)
        => RunAnalysis(path, ".NLA", St7.stNonlinearStatic, overwrite,
            NonlinearConfiguration(loadCase, steps, factor, geometry, material));

    public static Task<string> RunNonlinearStaticAsync(string path, bool overwrite, int loadCase, int steps, double factor,
        bool geometry, bool material, CancellationToken cancellation = default)
        => RunAnalysisAsync(path, ".NLA", St7.stNonlinearStatic, overwrite,
            NonlinearConfiguration(loadCase, steps, factor, geometry, material), cancellation);

    private static Action<int> NonlinearConfiguration(int loadCase, int steps, double factor, bool geometry, bool material)
    {
        if (loadCase < 1 || steps < 1 || steps > 10000 || !double.IsFinite(factor)) throw new ArgumentException("Use a positive load case, 1–10000 steps and a finite final load factor.");
        return id => {
            int count=0; Check(St7.St7GetNumLoadCase(id, ref count), "Read load cases");
            if (loadCase > count) throw new ArgumentException("Load case is outside the model range.");
            int increments=0; Check(St7.St7GetNumNLAIncrements(id, 0, ref increments), "Read nonlinear increments");
            for (int i=increments;i>=1;i--) Check(St7.St7DeleteNLAIncrement(id,0,i), "Replace nonlinear increment table");
            Check(St7.St7SetSolverDefaultsLogical(id,St7.spNonlinearGeometry,(byte)(geometry?St7.btTrue:St7.btFalse)), "Set nonlinear geometry");
            Check(St7.St7SetSolverDefaultsLogical(id,St7.spNonlinearMaterial,(byte)(material?St7.btTrue:St7.btFalse)), "Set nonlinear material");
            for(int i=1;i<=steps;i++)
            {
                Check(St7.St7AddNLAIncrement(id,0,$"Step {i}"), "Add nonlinear increment");
                Check(St7.St7SetNLALoadIncrementFactor(id,0,i,loadCase,factor*i/steps), "Set increment load factor");
                Check(St7.St7SetNLAFreedomIncrementFactor(id,0,i,1,1.0), "Set freedom case factor");
            }
            // R31 needs at least one increment before cases can be enabled.
            for (int i=1;i<=count;i++) Check(i==loadCase?St7.St7EnableNLALoadCase(id,0,i):St7.St7DisableNLALoadCase(id,0,i), "Set nonlinear active load case");
            Check(St7.St7EnableNLAFreedomCase(id,0,1), "Enable freedom case 1");
        };
    }

    public static double[][] ModalResults(string model, string result, int[] modes)
    {
        using var session = Results(model, result, modes.DefaultIfEmpty(1).Max());
        return modes.Select(mode => {
            if(mode < 1) throw new ArgumentException("Mode numbers start at 1.");
            var values=new double[16];Check(St7.St7GetModalResultsNFA(session.Id,mode,values), "Read modal frequency and participation");return values;
        }).ToArray();
    }

    public static double[] BucklingFactors(string model, string result, int[] modes)
    {
        using var session = Results(model, result, modes.DefaultIfEmpty(1).Max());
        return modes.Select(mode => {
            if(mode < 1) throw new ArgumentException("Mode numbers start at 1.");
            double factor=0;Check(St7.St7GetBucklingFactor(session.Id,mode,ref factor), "Read buckling factor");return factor;
        }).ToArray();
    }
}
