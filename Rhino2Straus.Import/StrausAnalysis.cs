using System;
using System.IO;
using System.Linq;
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
        Check(St7.St7RunSolver(session.Id, solver, St7.smBackgroundRun, St7.btTrue), "Run Straus7 solver");
        if (!File.Exists(result) || (existed && File.GetLastWriteTimeUtc(result) <= previous))
            throw new IOException("The solver did not produce fresh results. Inspect its log for convergence or licence errors.");
        return result;
    }

    public static string RunNaturalFrequency(string path, bool overwrite, int modes, double shift)
    {
        if (modes < 1 || !double.IsFinite(shift)) throw new ArgumentException("Use a positive mode count and finite shift in Hz.");
        return RunAnalysis(path, ".NFA", St7.stNaturalFrequency, overwrite, id => {
            Check(St7.St7SetNFANumModes(id, modes), "Set mode count");
            Check(St7.St7SetNFAShift(id, shift), "Set frequency shift");
            Check(St7.St7SetNFAModeParticipationCalculate(id, St7.btTrue), "Enable modal participation");
        });
    }

    public static string RunLinearBuckling(string path, bool overwrite, int modes, string initialResults, int variableCase, int fixedCase, double shift, AnalysisSelection selection = null)
    {
        if (modes < 1 || variableCase < 1 || fixedCase < 0 || !double.IsFinite(shift) || !File.Exists(initialResults)) throw new ArgumentException("Buckling needs initial static results, a positive variable case and mode count; fixed case 0 means none.");
        return RunAnalysis(path, ".LBA", St7.stLinearBuckling, overwrite, id => {
            if (selection != null) ConfigureStaticSelection(id, selection);
            Check(St7.St7SetLBANumModes(id, modes), "Set buckling mode count");
            Check(St7.St7SetLBAShift(id, shift), "Set buckling shift");
            Check(St7.St7SetLBAInitial(id, Path.GetFullPath(initialResults), variableCase, fixedCase), "Set initial stress results");
        });
    }

    public static string RunNonlinearStatic(string path, bool overwrite, int loadCase, int steps, double factor, bool geometry, bool material)
    {
        if (loadCase < 1 || steps < 1 || steps > 10000 || !double.IsFinite(factor)) throw new ArgumentException("Use a positive load case, 1–10000 steps and a finite final load factor.");
        return RunAnalysis(path, ".NLA", St7.stNonlinearStatic, overwrite, id => {
            int count=0; Check(St7.St7GetNumLoadCase(id, ref count), "Read load cases");
            if (loadCase > count) throw new ArgumentException("Load case is outside the model range.");
            int increments=0; Check(St7.St7GetNumNLAIncrements(id, 0, ref increments), "Read nonlinear increments");
            for (int i=increments;i>=1;i--) Check(St7.St7DeleteNLAIncrement(id,0,i), "Replace nonlinear increment table");
            for (int i=1;i<=count;i++) Check(i==loadCase?St7.St7EnableNLALoadCase(id,0,i):St7.St7DisableNLALoadCase(id,0,i), "Set nonlinear active load case");
            Check(St7.St7SetSolverDefaultsLogical(id,St7.spNonlinearGeometry,(byte)(geometry?St7.btTrue:St7.btFalse)), "Set nonlinear geometry");
            Check(St7.St7SetSolverDefaultsLogical(id,St7.spNonlinearMaterial,(byte)(material?St7.btTrue:St7.btFalse)), "Set nonlinear material");
            for(int i=1;i<=steps;i++)
            {
                Check(St7.St7AddNLAIncrement(id,0,$"Step {i}"), "Add nonlinear increment");
                Check(St7.St7SetNLALoadIncrementFactor(id,0,i,loadCase,factor*i/steps), "Set increment load factor");
                Check(St7.St7SetNLAFreedomIncrementFactor(id,0,i,1,1.0), "Set freedom case factor");
            }
        });
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
