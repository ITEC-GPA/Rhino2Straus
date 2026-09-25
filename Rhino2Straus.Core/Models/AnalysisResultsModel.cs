using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Rhino2Straus.Core.Models;

public enum ResultEntity { Node, Beam, Plate, Solid, Link }
public enum ResultQuantity { Displacement, Reaction, Force, Moment, Stress, Strain, TotalStrain, Curvature, NodeDisplacement, NodeReaction }
public sealed record ResultCaseInfo(int Number, string Name, bool Secondary, double? FrequencyHz = null, double? BucklingFactor = null);
public sealed class ModalResultInfo
{
    public int CaseNumber { get; }
    public ReadOnlyCollection<double> Values { get; }
    public ModalResultInfo(int caseNumber, IEnumerable<double> values)
    {
        CaseNumber = caseNumber; Values = Array.AsReadOnly(values.ToArray());
        if (caseNumber < 1 || Values.Count != 16 || Values.Any(x => !double.IsFinite(x))) throw new ArgumentException("Invalid modal result metadata.");
    }
}
public sealed record ResultIssue(int CaseNumber, ResultEntity Entity, int EntityId, ResultQuantity Quantity, int Surface, string Message);
/// <summary>Section properties actually used by Straus7, in Model units; angle in radians.</summary>
public sealed record NativeBeamSection(int PropertyId, int NativePropertyNumber, double Area, double I11, double I22,
    double J, double ShearOffset1, double ShearOffset2, double ShearArea1, double ShearArea2, double CentroidX, double CentroidY, double PrincipalAngle);

/// <summary>Immutable result table; IDs are Model IDs, NativeNumber is the Straus7 number.</summary>
public sealed class ElementResult
{
    public int CaseNumber { get; }
    public ResultEntity Entity { get; }
    public int EntityId { get; }
    public int NativeNumber { get; }
    public ResultQuantity Quantity { get; }
    public int Surface { get; }
    public string Axes { get; }
    public string Sampling { get; }
    public ReadOnlyCollection<string> Columns { get; }
    public ReadOnlyCollection<double> Positions { get; }
    public ReadOnlyCollection<ReadOnlyCollection<double>> Values { get; }
    public ElementResult(int caseNumber, ResultEntity entity, int entityId, int nativeNumber, ResultQuantity quantity,
        int surface, string axes, string sampling, IEnumerable<string> columns, IEnumerable<double> positions, IEnumerable<IEnumerable<double>> values)
    {
        CaseNumber = caseNumber; Entity = entity; EntityId = entityId; NativeNumber = nativeNumber;
        Quantity = quantity; Surface = surface; Axes = axes; Sampling = sampling;
        Columns = Array.AsReadOnly(columns.ToArray()); Positions = Array.AsReadOnly(positions.ToArray());
        Values = Array.AsReadOnly(values.Select(v => Array.AsReadOnly(v.ToArray())).ToArray());
        if (caseNumber < 1 || entityId < 1 || nativeNumber < 1 || Values.Count == 0 || Columns.Count == 0 ||
            Positions.Count != Values.Count || Positions.Any(x => !double.IsFinite(x)) ||
            Values.Any(v => v.Count != Columns.Count || v.Any(x => !double.IsFinite(x))))
            throw new ArgumentException("Invalid or non-finite result table.");
    }
    public override string ToString() => $"{Entity} {EntityId}: {Quantity}, case {CaseNumber}, {Values.Count} samples";
}

/// <summary>Read-only results suitable for a downstream verifier. No missing result is replaced with zero.</summary>
public sealed class AnalysisResultsModel
{
    public string Fingerprint { get; }
    public string ModelPath { get; }
    public string ResultPath { get; }
    public string Solver { get; }
    public DateTime ReadAtUtc { get; } = DateTime.UtcNow;
    public bool ProvenanceVerified { get; }
    public ReadOnlyCollection<ResultCaseInfo> Cases { get; }
    public ReadOnlyCollection<ElementResult> Records { get; }
    public ReadOnlyCollection<ResultIssue> Issues { get; }
    public ReadOnlyCollection<NativeBeamSection> BeamSections { get; }
    public ReadOnlyCollection<ModalResultInfo> Modal { get; }
    public bool IsComplete => Issues.Count == 0;
    public AnalysisResultsModel(string fingerprint, string modelPath, string resultPath, string solver, bool provenanceVerified,
        IEnumerable<ResultCaseInfo> cases, IEnumerable<ElementResult> records, IEnumerable<ResultIssue> issues, IEnumerable<NativeBeamSection> beamSections = null, IEnumerable<ModalResultInfo> modal = null)
    {
        Fingerprint = fingerprint; ModelPath = modelPath; ResultPath = resultPath; Solver = solver; ProvenanceVerified = provenanceVerified;
        Cases = Array.AsReadOnly(cases.ToArray()); Records = Array.AsReadOnly(records.ToArray()); Issues = Array.AsReadOnly(issues.ToArray());
        BeamSections = Array.AsReadOnly(beamSections?.ToArray() ?? Array.Empty<NativeBeamSection>());
        Modal = Array.AsReadOnly(modal?.ToArray() ?? Array.Empty<ModalResultInfo>());
        if (Cases.Select(c => c.Number).Distinct().Count() != Cases.Count || Records.Any(r => !Cases.Any(c => c.Number == r.CaseNumber)))
            throw new ArgumentException("Result case mapping is inconsistent.");
    }
    public void ValidateFor(ModelModel model, bool requireComplete = false, bool requireProvenance = false)
    {
        if (Fingerprint != ModelFingerprint.Compute(model)) throw new InvalidOperationException("Results are stale: geometry, properties, cases or loads have changed. Run the model again.");
        if (requireComplete && !IsComplete) throw new InvalidOperationException("Some result quantities are unavailable. Inspect Issues before verification.");
        if (requireProvenance && !ProvenanceVerified) throw new InvalidOperationException("Result provenance has not been verified.");
    }
    public IEnumerable<ElementResult> Select(ModelModel model, ResultEntity entity, ResultQuantity quantity, int caseNumber = 0, IEnumerable<int> ids = null, int surface = -1)
    {
        ValidateFor(model);
        if (caseNumber < 0 || (caseNumber > 0 && !Cases.Any(c => c.Number == caseNumber))) throw new ArgumentException("Unknown result case number.");
        var selected = ids?.ToHashSet();
        return Records.Where(r => r.Entity == entity && r.Quantity == quantity && (caseNumber == 0 || r.CaseNumber == caseNumber)
            && (selected is null || selected.Count == 0 || selected.Contains(r.EntityId)) && (surface < 0 || r.Surface == surface));
    }
    public override string ToString() => $"{Solver}: {Cases.Count} cases, {Records.Count} tables, {Issues.Count} unavailable quantities";
}
