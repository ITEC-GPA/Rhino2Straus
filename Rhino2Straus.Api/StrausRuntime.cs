using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using St7API;

namespace Rhino2Straus.Api;

/// <summary>One native API session at a time, shared by export, import and results.</summary>
public static class StrausRuntime
{
    internal static readonly object Gate = new();
    private static readonly SemaphoreSlim analysisQueue = new(1, 1);
    private static readonly AsyncLocal<object> operationOwner = new();
    private static object activeOperation;
    private static string solverWaitDiagnostic;
    public static string SolverWaitDiagnostic => Volatile.Read(ref solverWaitDiagnostic);
    /// <summary>A polling failure retains the native owner until the SDK confirms termination.</summary>
    public static void ReportSolverWaitProblem(string message) => Volatile.Write(ref solverWaitDiagnostic, message);

    /// <summary>Serializes native pipelines on the calling UI context. Only the external solver
    /// runs concurrently; all API calls and session disposal stay on their owning thread.</summary>
    public static async Task<T> RunExclusiveAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken cancellation = default)
    {
        ArgumentNullException.ThrowIfNull(work);
        if (SynchronizationContext.Current == null || SynchronizationContext.Current.GetType() == typeof(SynchronizationContext))
            throw new InvalidOperationException("Async Straus7 operations require the host UI synchronization context.");
        if (operationOwner.Value != null && ReferenceEquals(operationOwner.Value, Volatile.Read(ref activeOperation)))
            return await work(cancellation);
        await analysisQueue.WaitAsync(cancellation);
        var owner = new object();
        try
        {
            // Never wait synchronously for a native session owned by another thread.
            while (!Monitor.TryEnter(Gate)) await Task.Delay(25, cancellation);
            try { Volatile.Write(ref activeOperation, owner); operationOwner.Value = owner; }
            finally { Monitor.Exit(Gate); }
            cancellation.ThrowIfCancellationRequested();
            T value = await work(cancellation);
            cancellation.ThrowIfCancellationRequested();
            return value;
        }
        finally
        {
            if (ReferenceEquals(Volatile.Read(ref activeOperation), owner)) Volatile.Write(ref activeOperation, null);
            operationOwner.Value = null;
            ReportSolverWaitProblem(null);
            analysisQueue.Release();
        }
    }

    internal static void EnterWithoutWaiting()
    {
        var owner = Volatile.Read(ref activeOperation);
        if (owner != null && !ReferenceEquals(operationOwner.Value, owner)) throw new StrausBusyException();
        if (!Monitor.TryEnter(Gate)) throw new StrausBusyException();
        // The owner can change between the first check and acquisition.
        owner = Volatile.Read(ref activeOperation);
        if (owner != null && !ReferenceEquals(operationOwner.Value, owner))
        {
            Monitor.Exit(Gate);
            throw new StrausBusyException();
        }
    }

    private static bool initialized;
    private static bool configured;
    private static readonly HashSet<int> leased = new();
    public static string LoadedLibrary { get; private set; }

    public static void Configure()
    {
        EnterWithoutWaiting();
        try
        {
            if (configured) return;
            NativeLibrary.SetDllImportResolver(typeof(St7).Assembly, Resolve);
            configured = true;
        }
        finally { Monitor.Exit(Gate); }
    }

    private static IntPtr Resolve(string name, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!name.Equals("St7api.dll", StringComparison.OrdinalIgnoreCase)) return IntPtr.Zero;
        if (!Environment.Is64BitProcess)
            throw new PlatformNotSupportedException("Straus7 R3 requires a 64-bit process.");
        var candidates = new List<string>();
        var diagnostics = new List<string>();
        string custom = Environment.GetEnvironmentVariable("STRAUS7_API_PATH");
        if (!string.IsNullOrWhiteSpace(custom))
            candidates.Add(Directory.Exists(custom) ? Path.Combine(custom, name) : custom);
        // Assembly.Location is the plugin folder; AppContext.BaseDirectory is normally Rhino's folder.
        string assemblyFolder = Path.GetDirectoryName(assembly.Location);
        if (!string.IsNullOrEmpty(assemblyFolder)) candidates.Add(Path.Combine(assemblyFolder, name));
        string programs = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        foreach (string folder in new[] { "Straus7 R31", "Strand7 R31", "Straus7 R3", "Strand7 R3" })
            candidates.Add(Path.Combine(programs, folder, "Bin64", name));
        candidates.Add(Path.Combine(AppContext.BaseDirectory, name));
        foreach (string path in candidates)
        {
            if (!File.Exists(path)) { diagnostics.Add(path + ": file not found"); continue; }
            try
            {
                var handle = NativeLibrary.Load(path);
                LoadedLibrary = Path.GetFullPath(path);
                return handle;
            }
            catch (Exception error) when (error is DllNotFoundException or BadImageFormatException or FileLoadException)
            {
                diagnostics.Add(path + ": " + error.Message);
            }
        }
        try
        {
            var fallback = NativeLibrary.Load(name, assembly, searchPath);
            LoadedLibrary = name;
            return fallback;
        }
        catch (Exception error) when (error is DllNotFoundException or BadImageFormatException or FileLoadException)
        {
            diagnostics.Add("System DLL search: " + error.Message);
            throw new DllNotFoundException("Unable to load Straus7 R3 x64 API. Set STRAUS7_API_PATH to its Bin64 directory or St7api.dll, then restart Rhino. "
                + "A present DLL may have an incompatible architecture or missing dependencies. Attempts:" + Environment.NewLine
                + string.Join(Environment.NewLine, diagnostics), error);
        }
    }

    /// <summary>For an exclusive host shutting down, after all jobs have drained. Not for closing a GH document.</summary>
    public static void ReleaseForHostShutdown()
    {
        EnterWithoutWaiting();
        try
        {
            if (!initialized) return;
            if (leased.Count != 0) throw new InvalidOperationException("Cannot release Straus7 while sessions are open.");
            int code = St7.St7Release();
            if (code != 0) throw new InvalidOperationException($"Release Straus7 API failed ({code}).");
            initialized = false;
        }
        finally { Monitor.Exit(Gate); }
    }

    internal static void Initialize()
    {
        Configure();
        if (initialized) return;
        Check(St7.St7Init(), $"Initialize Straus7 API (an API licence is required; loaded library: {LoadedLibrary})");
        initialized = true;
    }

    internal static int Lease()
    {
        // Probe open files as other Grasshopper plug-ins may use the same native library.
        for (int id = 32; id >= 1; id--)
        {
            if (leased.Contains(id)) continue;
            if (St7.St7GetUnits(id, new int[St7.kLastUnit]) == 0) continue;
            leased.Add(id);
            return id;
        }
        throw new InvalidOperationException("All 32 Straus7 API model slots are in use.");
    }
    internal static void Release(int id) => leased.Remove(id);

    public static void Check(int code, string operation)
    {
        if (code == 0) return;
        var text = new StringBuilder(1024);
        try
        {
            // Public callers outside a session must not race the native error formatter.
            if (!Monitor.IsEntered(Gate)) throw new InvalidOperationException("Native gate is not owned");
            if (St7.St7GetAPIErrorString(code, text, text.Capacity) != 0)
                St7.St7GetSolverErrorString(code, text, text.Capacity);
        }
        catch { /* Keep the original code if the native error formatter is unavailable. */ }
        throw new InvalidOperationException($"{operation}: {text} (Straus7 error {code}).");
    }
}

public sealed class StrausSession : IDisposable
{
    public int Id { get; private set; }
    private bool open;
    private bool disposed;
    public bool ResultsOpen { get; private set; }

    public StrausSession(string path, bool create = false, bool readOnly = true)
    {
        StrausRuntime.EnterWithoutWaiting();
        try
        {
            if (!Environment.Is64BitProcess) throw new PlatformNotSupportedException("Straus7 R3 requires a 64-bit process.");
            path = Path.GetFullPath(path);
            if (!create && !File.Exists(path)) throw new FileNotFoundException("Straus7 model does not exist.", path);
            StrausRuntime.Initialize();
            Id = StrausRuntime.Lease();
            int code = create ? St7.St7NewFile(Id, path, Path.GetTempPath()) :
                readOnly ? St7.St7OpenFileReadOnly(Id, path, Path.GetTempPath()) : St7.St7OpenFile(Id, path, Path.GetTempPath());
            StrausRuntime.Check(code, create ? "Create model" : "Open model");
            open = true;
        }
        catch
        {
            if (Id > 0) StrausRuntime.Release(Id);
            Monitor.Exit(StrausRuntime.Gate);
            throw;
        }
    }

    private void RequireOwner()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!Monitor.IsEntered(StrausRuntime.Gate))
            throw new InvalidOperationException("A Straus7 session must be used and disposed on its owning thread.");
    }

    public (int Primary, int Secondary) OpenResults(string path, int combination = -1)
    {
        RequireOwner();
        if (!File.Exists(path)) throw new FileNotFoundException("Result file does not exist.", path);
        // Always regenerate LSA combinations from this model's factors, never reuse a stale .LSC.
        if (combination < 0) combination = Path.GetExtension(path).Equals(".LSA", StringComparison.OrdinalIgnoreCase) ? St7.kGenerateNewCombinations : St7.kNoCombinations;
        int primary = 0, secondary = 0;
        StrausRuntime.Check(St7.St7OpenResultFile(Id, Path.GetFullPath(path), "", combination, ref primary, ref secondary), "Open results");
        ResultsOpen = true;
        return (primary, secondary);
    }

    public void Save() { RequireOwner(); StrausRuntime.Check(St7.St7SaveFile(Id), "Save model"); }

    public void Close()
    {
        RequireOwner();
        if (ResultsOpen) { StrausRuntime.Check(St7.St7CloseResultFile(Id), "Close results"); ResultsOpen = false; }
        if (open) { StrausRuntime.Check(St7.St7CloseFile(Id), "Close model"); open = false; }
    }

    public void Dispose()
    {
        if (disposed) return;
        RequireOwner();
        try { Close(); }
        finally { disposed = true; StrausRuntime.Release(Id); Monitor.Exit(StrausRuntime.Gate); }
    }
}

/// <summary>Native API is occupied; callers on the UI thread must never wait behind a solver.</summary>
public sealed class StrausBusyException : InvalidOperationException
{
    public StrausBusyException() : base("Straus7 API is busy with a background analysis. Offline Grasshopper operations remain available; retry this native operation when the analysis finishes." + (StrausRuntime.SolverWaitDiagnostic == null ? "" : " " + StrausRuntime.SolverWaitDiagnostic)) { }
}
