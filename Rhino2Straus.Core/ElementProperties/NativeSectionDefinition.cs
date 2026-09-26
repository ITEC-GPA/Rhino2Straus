using System;
using System.Linq;
using St7API;
using Rhino2Straus.Core.Settings;

namespace Rhino2Straus.Core.ElementProperties;

public sealed class NativeSectionOptions
{
    public int Divisions { get; set; } = 24;
    public int CircularDivisions { get; set; }
    public bool IncludeShear { get; set; } = true;
    public void Validate()
    {
        if (Divisions < 4 || Divisions > 200) throw new ArgumentException("Section divisions must be 4..200.");
        if (CircularDivisions < 0 || CircularDivisions > 6) throw new ArgumentException("Circular divisions: 0=Auto, 1..6=8..48.");
    }
    public NativeSectionOptions Copy() => new() { Divisions=Divisions, CircularDivisions=CircularDivisions, IncludeShear=IncludeShear };
}

/// <summary>Vendor shape ID and dimensions, retained through copy/merge/export. Angles in BGL arrays are degrees.</summary>
public sealed class NativeSectionDefinition : IEquatable<NativeSectionDefinition>
{
    public bool IsBgl { get; set; }
    public int Shape { get; set; }
    public double[] Dimensions { get; set; } = Array.Empty<double>();
    public ModelUnitsModel.LengthUnitTypes LengthUnit { get; set; }
    public NativeSectionOptions Options { get; set; } = new();
    public NativeSectionDefinition Copy() => new() { IsBgl=IsBgl, Shape=Shape, Dimensions=(double[])Dimensions.Clone(), LengthUnit=LengthUnit, Options=Options.Copy() };
    public void Validate()
    {
        Options.Validate();
        if (!Enum.IsDefined(LengthUnit)) throw new ArgumentException("Invalid section length unit.");
        bool standard = Shape >= St7.bsCircularSolid && Shape <= St7.bsCruciform && Shape != St7.bsBXSSection;
        if (IsBgl ? Shape < St7.bgRectangularHollow || Shape > St7.bgBulbFlat : !standard)
            throw new ArgumentException("Unsupported native section shape.");
        if (Dimensions == null || Dimensions.Length != (IsBgl ? 16 : 6) || Dimensions.Any(x => !double.IsFinite(x)))
            throw new ArgumentException("Section dimensions must be finite; standard=6, BGL=16 values.");
        if (Dimensions[0] <= 0 || (Shape != St7.bsCircularSolid && !IsBgl && Shape != St7.bsCircularHollow && Dimensions[1] <= 0) || (IsBgl && Dimensions[1] <= 0))
            throw new ArgumentException("Section overall dimensions must be positive.");
        // Straus7 performs the shape-specific checks, including radii and tapered flanges.
    }
    public bool Equals(NativeSectionDefinition other) => other != null && IsBgl==other.IsBgl && Shape==other.Shape && LengthUnit==other.LengthUnit &&
        Dimensions.SequenceEqual(other.Dimensions) && Options.Divisions==other.Options.Divisions && Options.CircularDivisions==other.Options.CircularDivisions && Options.IncludeShear==other.Options.IncludeShear;
    public override bool Equals(object obj) => Equals(obj as NativeSectionDefinition);
    public override int GetHashCode() => HashCode.Combine(IsBgl,Shape,LengthUnit);
}
