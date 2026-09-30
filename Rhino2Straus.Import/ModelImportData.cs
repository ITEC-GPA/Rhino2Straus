using System;
using System.Collections.Generic;
using System.Linq;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Models;
using Rhino2Straus.Core.Settings;

namespace Rhino2Straus.Import;

public sealed record ImportIdentity(string Entity, int NativeNumber, int NativeLabel, int ModelId);
public sealed record ImportIssue(string Owner, string Detail, bool DataOmitted = true)
{
    public override string ToString() => $"{Owner}: {Detail}";
}

/// <summary>Native data is read separately from assembly so topology/partial-import rules can be tested without Straus.</summary>
public sealed class ModelImportData
{
    public List<NodeElementModel> Nodes { get; } = new();
    public List<FrameElementModel> Beams { get; } = new();
    public List<AreaElementModel> Plates { get; } = new();
    public List<SolidElementModel> Solids { get; } = new();
    public List<LinkElementModel> Links { get; } = new();
    public List<LoadCaseModel> Cases { get; } = new();
    public List<LoadCombinationModel> Combinations { get; } = new();
    public List<ImportIdentity> Identities { get; } = new();
    public List<ImportIssue> Issues { get; } = new();
    public SelfWeightModel SelfWeight { get; set; }

    public ModelImportResult Assemble(double tolerance, bool allowPartial)
    {
        if (!double.IsFinite(tolerance) || tolerance <= 0) throw new ArgumentOutOfRangeException(nameof(tolerance));
        var model = new ModelModel { PreserveCoincidentNodes = true };
        model.ModelUnits.Tolerance = tolerance;
        model.BuildModel(Nodes, Beams, Plates, Links, Combinations, SelfWeight, Solids);
        // Keep unused native load cases too; Build collects only referenced cases.
        foreach (var lc in Cases)
        {
            if (model.LoadCases.TryGetValue(lc.Name, out var stored)) stored.Id = lc.Id;
            else model.LoadCases.Add(new LoadCaseModel(lc));
        }
        var issues = new List<ImportIssue>(Issues);
        var validation = ModelValidator.Validate(model);
        foreach (string error in validation.Errors) issues.Add(new("Preflight", error));
        foreach (string warning in validation.Warnings) issues.Add(new("Preflight", warning, false));
        bool complete = !issues.Any(i => i.DataOmitted);
        return new(complete || allowPartial ? model : null, complete, Identities.ToArray(), issues.ToArray());
    }
}

public sealed record ModelImportResult(ModelModel Model, bool Complete, ImportIdentity[] Identities, ImportIssue[] Issues);
