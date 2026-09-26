using System;

namespace Rhino2Straus.Core.Settings;

[Flags]
public enum PreviewLayer
{
    None=0, Nodes=1, Beams=2, Plates=4, Solids=8, Links=16, Sections=32,
    Loads=64, Supports=128, Releases=256, Springs=512, Masses=1024,
    Prescribed=2048, Offsets=4096, Axes=8192, Labels=16384, Properties=32768,
    Values=65536, Results=131072, All=262143
}

/// <summary>Display-only settings; never attached to or used to modify the analysis Model.</summary>
public sealed record PreviewSettings
{
    public bool Enabled { get; init; } = true;
    public bool Shaded { get; init; } = true;
    public PreviewLayer Layers { get; init; } = PreviewLayer.All & ~(PreviewLayer.Axes|PreviewLayer.Labels|PreviewLayer.Properties|PreviewLayer.Results);
    public double SymbolScale { get; init; } = 1;
    public double LoadScale { get; init; } = 1;
    public double Transparency { get; init; } = .3;
    public bool Shows(PreviewLayer layer) => Enabled && (Layers & layer)==layer;
    public void Validate()
    {
        if(!double.IsFinite(SymbolScale)||SymbolScale<=0||!double.IsFinite(LoadScale)||LoadScale<=0)
            throw new ArgumentException("Preview symbol and load scales must be finite and positive.");
        if(!double.IsFinite(Transparency)||Transparency<0||Transparency>1)throw new ArgumentException("Transparency must be 0..1.");
        if((Layers & ~PreviewLayer.All)!=0)throw new ArgumentException("Unknown preview layer.");
    }
}
