using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using St7API;

namespace Rhino2Straus.Api;

/// <summary>One native API session at a time, shared by export, import and results.</summary>
public static class StrausRuntime
{
    internal static readonly object Gate = new();
    private static bool initialized;
    private static bool configured;
    private static readonly HashSet<int> leased = new();
    public static string LoadedLibrary { get; private set; }

    public static void Configure()
    {
        lock (Gate)
        {
            if (configured) return;
            NativeLibrary.SetDllImportResolver(typeof(St7).Assembly, Resolve);
            configured = true;
        }
    }

    private static IntPtr Resolve(string name, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!name.Equals("St7api.dll", StringComparison.OrdinalIgnoreCase)) return IntPtr.Zero;
        var candidates = new List<string>();
        string custom = Environment.GetEnvironmentVariable("STRAUS7_API_PATH");
        if (!string.IsNullOrWhiteSpace(custom)) candidates.Add(Directory.Exists(custom) ? Path.Combine(custom, name) : custom);
        string programs = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        foreach (string folder in new[] { "Straus7 R31", "Strand7 R31", "Straus7 R3", "Strand7 R3" })
            candidates.Add(Path.Combine(programs, folder, "Bin64", name));
        candidates.Add(Path.Combine(AppContext.BaseDirectory, name));
        foreach (string path in candidates)
            if (File.Exists(path) && NativeLibrary.TryLoad(path, out var handle)) { LoadedLibrary = path; return handle; }
        if (NativeLibrary.TryLoad(name, assembly, searchPath, out var fallback)) { LoadedLibrary = name; return fallback; }
        throw new DllNotFoundException("Straus7 R3 x64 API was not found. Set STRAUS7_API_PATH to its Bin64 directory or St7api.dll, then restart Rhino.");
    }

    internal static void Initialize()
    {
        Configure();
        if (initialized) return;
        Check(St7.St7Init(), "Initialize Straus7 API (an API licence is required)");
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
        Monitor.Enter(StrausRuntime.Gate);
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

    public (int Primary, int Secondary) OpenResults(string path, int combination = -1)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Result file does not exist.", path);
        // Always regenerate LSA combinations from this model's factors, never reuse a stale .LSC.
        if (combination < 0) combination = Path.GetExtension(path).Equals(".LSA", StringComparison.OrdinalIgnoreCase) ? St7.kGenerateNewCombinations : St7.kNoCombinations;
        int primary = 0, secondary = 0;
        StrausRuntime.Check(St7.St7OpenResultFile(Id, Path.GetFullPath(path), "", combination, ref primary, ref secondary), "Open results");
        ResultsOpen = true;
        return (primary, secondary);
    }

    public void Save() => StrausRuntime.Check(St7.St7SaveFile(Id), "Save model");

    public void Close()
    {
        if (ResultsOpen) { StrausRuntime.Check(St7.St7CloseResultFile(Id), "Close results"); ResultsOpen = false; }
        if (open) { StrausRuntime.Check(St7.St7CloseFile(Id), "Close model"); open = false; }
    }

    public void Dispose()
    {
        if (disposed) return;
        try { Close(); }
        finally { disposed = true; StrausRuntime.Release(Id); Monitor.Exit(StrausRuntime.Gate); }
    }
}
