using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Rhino2Straus.Core.Models;

namespace Rhino2Straus.Import;

/// <summary>Cheap file identity checks. SHA-256 manifests remain the authority when reading Model results.</summary>
public sealed record AnalysisFileState(string Path, bool Exists, long Length, DateTime Written)
{
    public static AnalysisFileState Capture(string path)
    {
        var file = new FileInfo(System.IO.Path.GetFullPath(path));
        return new(file.FullName, file.Exists, file.Exists ? file.Length : 0,
            file.Exists ? file.LastWriteTimeUtc : DateTime.MinValue);
    }

    public bool IsCurrent()
    {
        try { return this == Capture(Path); }
        catch (IOException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
    }

    public void RequireUnchanged()
    {
        if (!Exists || !IsCurrent())
            throw new IOException("Analysis input file is missing or changed while the job was queued: " + Path + ". Run again with the current file.");
    }
}

/// <summary>Capture outputs on the worker before releasing the native gate; never touch GH from here.</summary>
public static class AnalysisOutputFiles
{
    private sealed record Stamp(AnalysisFileState[] Files);
    private static readonly ConditionalWeakTable<object, Stamp> stamps = new();
    private static long revision;
    public static void NotifyChanged() => Interlocked.Increment(ref revision);
    public static long Revision => Interlocked.Read(ref revision);

    public static T Track<T>(T value) where T : class
    {
        string[] paths = value switch
        {
            ModelModel { Results: not null } model => new[] { model.Results.ModelPath, model.Results.ResultPath,
                ResultProvenance.ManifestPath(model.Results.ResultPath) },
            string path when !string.IsNullOrEmpty(path) => new[] { path, System.IO.Path.ChangeExtension(path, ".st7") },
            _ => Array.Empty<string>()
        };
        if (value != null)
        {
            stamps.Remove(value);
            stamps.Add(value, new Stamp(paths.Select(AnalysisFileState.Capture).ToArray()));
        }
        Interlocked.Increment(ref revision);
        return value;
    }

    public static bool IsCurrent(object value) =>
        value == null || !stamps.TryGetValue(value, out var stamp) || stamp.Files.All(file => file.IsCurrent());
}
