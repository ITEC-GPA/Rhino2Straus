using System;
using Rhino2Straus.Core.Helper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Rhino2Straus.Api;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Models;
using St7API;

namespace Rhino2Straus.Import;

public sealed record AnalysisSelection(string[] Cases, string[] Combinations)
{
    /// <summary>Empty name lists mean all in the enabled family; combo dependencies are always included.</summary>
    public static AnalysisSelection Create(ModelModel model, IEnumerable<string> cases, IEnumerable<string> combinations, bool runCases = true, bool runCombinations = true)
    {
        string[] Pick(IEnumerable<string> requested, IEnumerable<string> available, bool enabled)
        {
            var all = available.ToArray(); var names = requested?.Distinct(StringComparer.Ordinal).ToArray() ?? Array.Empty<string>();
            if (names.Except(all, StringComparer.Ordinal).Any()) throw new ArgumentException("Unknown load case or combination: " + string.Join(", ", names.Except(all)));
            return enabled ? (names.Length == 0 ? all : names) : Array.Empty<string>();
        }
        var selectedCases = Pick(cases, model.LoadCases.Keys, runCases).ToHashSet(StringComparer.Ordinal);
        var selectedCombos = Pick(combinations, model.LoadCombinations.Keys, runCombinations);
        foreach (string combo in selectedCombos)
            foreach (var factor in model.LoadCombinations[combo].LoadFactorList)
                if (factor.Factor != 0) selectedCases.Add(factor.LoadCase.Name);
        if (selectedCases.Count == 0) throw new ArgumentException("Select at least one load case or a combination with non-zero factors.");
        return new(selectedCases.OrderBy(x => x, StringComparer.Ordinal).ToArray(), selectedCombos);
    }
}

/// <summary>Provenance is tied to both native files, never only to their names or timestamps.</summary>
public sealed record ResultProvenance(string Fingerprint, string ModelSha256, string ResultsSha256, string Solver)
{
    public static string HashFile(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)); }
    public static string ManifestPath(string results) => results + ".rhino2straus.json";
    public static void Save(string model, string results, string fingerprint, string solver)
    {
        var proof = new ResultProvenance(fingerprint, HashFile(model), HashFile(results), solver);
        File.WriteAllText(ManifestPath(results), JsonSerializer.Serialize(proof, new JsonSerializerOptions { WriteIndented = true }));
    }
    public static bool Verify(string model, string results, string fingerprint)
    {
        string file = ManifestPath(results);
        if (!File.Exists(file)) return false;
        try
        {
            var proof = JsonSerializer.Deserialize<ResultProvenance>(File.ReadAllText(file));
            return proof != null && proof.Fingerprint == fingerprint && proof.ModelSha256 == HashFile(model) && proof.ResultsSha256 == HashFile(results);
        }
        catch (JsonException) { return false; }
    }
}

public static class ModelAnalysisService
{
    private static void Check(int code, string operation) => StrausRuntime.Check(code, operation);

    private static ModelModel RunOther(ModelModel input, string path, string extension, bool overwrite, Func<string, string> solve, int stations, int sample)
    {
        ValidateSampling(stations, sample);
        var model = new ModelModel(input); model.ClearResults(); path = Path.GetFullPath(path);
        string result = Path.ChangeExtension(path, extension);
        if (!overwrite && (File.Exists(result) || File.Exists(ResultProvenance.ManifestPath(result)))) throw new IOException("Results already exist. Enable Overwrite to replace this analysis.");
        if (!model.CreateModelStraus7R3(path, out _, out _, out var errors, overwrite)) throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
        result = solve(path);
        ResultProvenance.Save(path, result, ModelFingerprint.Compute(model), extension.TrimStart('.'));
        return Read(model, path, result, stations, sample);
    }

    public static ModelModel RunModal(ModelModel model, string path, bool overwrite, int modes = 10, double shift = 0, int stations = 11, int sample = St7.spGaussPoints)
    {
        if (modes < 1 || !double.IsFinite(shift)) throw new ArgumentException("Use a positive mode count and a finite shift.");
        return RunOther(model, path, ".NFA", overwrite, p => StrausFileService.RunNaturalFrequency(p, overwrite, modes, shift), stations, sample);
    }

    public static ModelModel RunNonlinear(ModelModel model, string path, bool overwrite, string caseName, int steps = 10, double factor = 1,
        bool geometry = true, int stations = 11, int sample = St7.spGaussPoints)
    {
        int loadCase = model.LoadCases.Keys.ToList().IndexOf(caseName) + 1;
        if (loadCase < 1 || steps < 1 || steps > 10000 || !double.IsFinite(factor)) throw new ArgumentException("Choose an existing load case, 1..10000 increments and finite load factor.");
        // Input materials are elastic; do not imply that design strengths define plastic constitutive laws.
        return RunOther(model, path, ".NLA", overwrite, p => StrausFileService.RunNonlinearStatic(p, overwrite, loadCase, steps, factor, geometry, false), stations, sample);
    }

    public static ModelModel RunBuckling(ModelModel model, string path, bool overwrite, string initialResults, int variableCase, int fixedCase = 0,
        int modes = 10, double shift = 0, int stations = 11, int sample = St7.spGaussPoints)
    {
        var selection = ValidateBucklingInitial(model, initialResults, variableCase, fixedCase, modes, shift);
        return RunOther(model, path, ".LBA", overwrite, p => StrausFileService.RunLinearBuckling(p, overwrite, modes, initialResults, variableCase, fixedCase, shift, selection), stations, sample);
    }

    private static AnalysisSelection ValidateBucklingInitial(ModelModel model, string initialResults, int variableCase,
        int fixedCase, int modes, double shift)
    {
        if (variableCase < 1 || fixedCase < 0 || modes < 1 || !double.IsFinite(shift) || !File.Exists(initialResults)) throw new ArgumentException("Supply static results, a positive variable case and mode count, and a finite shift.");
        string manifest = ResultProvenance.ManifestPath(initialResults);
        if (!File.Exists(manifest)) throw new InvalidDataException("Generate initial LSA results with Run Model Cases and Combinations first.");
        var proof = JsonSerializer.Deserialize<ResultProvenance>(File.ReadAllText(manifest));
        if (!Path.GetExtension(initialResults).Equals(".LSA", StringComparison.OrdinalIgnoreCase) || proof == null || proof.Fingerprint != ModelFingerprint.Compute(model) || proof.ResultsSha256 != ResultProvenance.HashFile(initialResults))
            throw new InvalidDataException("Initial static results do not match this Model.");
        string initialModel = Path.ChangeExtension(initialResults, ".st7");
        if (!ResultProvenance.Verify(initialModel, initialResults, proof.Fingerprint)) throw new InvalidDataException("The native input for the initial static analysis changed. Run the static Model again.");
        return StrausFileService.ReadStaticSelection(initialModel, initialResults, variableCase, fixedCase);
    }

    public static ModelModel RunStatic(ModelModel input, string path, bool overwrite, IEnumerable<string> cases = null,
        IEnumerable<string> combinations = null, bool runCases = true, bool runCombinations = true, int stations = 11, int sample = St7.spGaussPoints)
    {
        ValidateSampling(stations, sample);
        var model = new ModelModel(input); model.ClearResults();
        var selection = AnalysisSelection.Create(model, cases, combinations, runCases, runCombinations);
        path = Path.GetFullPath(path);
        string result = Path.ChangeExtension(path, ".LSA");
        if (!overwrite && (File.Exists(result) || File.Exists(ResultProvenance.ManifestPath(result)))) throw new IOException("Results already exist. Enable Overwrite to replace this analysis.");
        if (!model.CreateModelStraus7R3(path, out _, out _, out var errors, overwrite)) throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
        result = StrausFileService.RunSelectedStatic(path, overwrite, selection);
        ResultProvenance.Save(path, result, ModelFingerprint.Compute(model), "Linear static");
        return Read(model, path, result, stations, sample);
    }

    /// <summary>UI-context pipeline: native export/read stay on the caller's thread; only the external solver is awaited.
    /// The caller must supply a stable Model snapshot. Cancellation never kills an executing solver.</summary>
    public static Task<ModelModel> RunStaticAsync(ModelModel input, string path, bool overwrite, IEnumerable<string> cases = null,
        IEnumerable<string> combinations = null, bool runCases = true, bool runCombinations = true, int stations = 11,
        int sample = St7.spGaussPoints, CancellationToken cancellation = default)
        => StrausRuntime.RunExclusiveAsync(async token =>
        {
            token.ThrowIfCancellationRequested();
            ValidateSampling(stations, sample);
            var selection = AnalysisSelection.Create(input, cases, combinations, runCases, runCombinations);
            return await RunOtherAsync(input, path, ".LSA", overwrite,
                p => StrausFileService.RunSelectedStaticAsync(p, overwrite, selection, token), stations, sample, token, "Linear static");
        }, cancellation);

    public static Task<ModelModel> RunModalAsync(ModelModel model, string path, bool overwrite, int modes = 10,
        double shift = 0, int stations = 11, int sample = St7.spGaussPoints, CancellationToken cancellation = default)
        => StrausRuntime.RunExclusiveAsync(async token =>
        {
            token.ThrowIfCancellationRequested();
            if (modes < 1 || !double.IsFinite(shift)) throw new ArgumentException("Use a positive mode count and a finite shift.");
            return await RunOtherAsync(model, path, ".NFA", overwrite,
                p => StrausFileService.RunNaturalFrequencyAsync(p, overwrite, modes, shift, token), stations, sample, token);
        }, cancellation);

    public static Task<ModelModel> RunNonlinearAsync(ModelModel model, string path, bool overwrite, string caseName,
        int steps = 10, double factor = 1, bool geometry = true, int stations = 11, int sample = St7.spGaussPoints,
        CancellationToken cancellation = default)
        => StrausRuntime.RunExclusiveAsync(async token =>
        {
            token.ThrowIfCancellationRequested();
            int loadCase = model.LoadCases.Keys.ToList().IndexOf(caseName) + 1;
            if (loadCase < 1 || steps < 1 || steps > 10000 || !double.IsFinite(factor))
                throw new ArgumentException("Choose an existing load case, 1..10000 increments and finite load factor.");
            return await RunOtherAsync(model, path, ".NLA", overwrite,
                p => StrausFileService.RunNonlinearStaticAsync(p, overwrite, loadCase, steps, factor, geometry, false, token),
                stations, sample, token);
        }, cancellation);

    public static Task<ModelModel> RunBucklingAsync(ModelModel model, string path, bool overwrite, string initialResults,
        int variableCase, int fixedCase = 0, int modes = 10, double shift = 0, int stations = 11,
        int sample = St7.spGaussPoints, CancellationToken cancellation = default)
        => StrausRuntime.RunExclusiveAsync(async token =>
        {
            token.ThrowIfCancellationRequested();
            ValidateSampling(stations, sample);
            initialResults = Path.GetFullPath(initialResults);
            var selection = ValidateBucklingInitial(model, initialResults, variableCase, fixedCase, modes, shift);
            token.ThrowIfCancellationRequested();
            return await RunOtherAsync(model, path, ".LBA", overwrite,
                p => StrausFileService.RunLinearBucklingAsync(p, overwrite, modes, initialResults, variableCase, fixedCase,
                    shift, selection, token), stations, sample, token);
        }, cancellation);

    private static async Task<ModelModel> RunOtherAsync(ModelModel input, string path, string extension, bool overwrite,
        Func<string, Task<string>> solve, int stations, int sample, CancellationToken cancellation, string solverName = null)
    {
        cancellation.ThrowIfCancellationRequested();
        ValidateSampling(stations, sample);
        var model = new ModelModel(input); model.ClearResults();
        path = Path.GetFullPath(path);
        string result = Path.ChangeExtension(path, extension);
        if (!overwrite && (File.Exists(result) || File.Exists(ResultProvenance.ManifestPath(result))))
            throw new IOException("Results already exist. Enable Overwrite to replace this analysis.");
        cancellation.ThrowIfCancellationRequested();
        if (!model.CreateModelStraus7R3(path, out _, out _, out var errors, overwrite))
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
        cancellation.ThrowIfCancellationRequested();
        result = await solve(path);
        cancellation.ThrowIfCancellationRequested();
        ResultProvenance.Save(path, result, ModelFingerprint.Compute(model), solverName ?? extension.TrimStart('.'));
        cancellation.ThrowIfCancellationRequested();
        var solved = Read(model, path, result, stations, sample);
        cancellation.ThrowIfCancellationRequested();
        return solved;
    }

    private static void ValidateSampling(int stations, int sample)
    {
        if (stations < 2 || stations > 100 || sample < 0 || sample > 4) throw new ArgumentException("Use 2..100 beam stations and a Straus7 sample mode 0..4.");
    }

    public static ModelModel Read(ModelModel input, string path, string result, int stations = 11, int sample = St7.spGaussPoints, bool allowUnverified = false)
    {
        ValidateSampling(stations, sample);
        var model = new ModelModel(input); model.ClearResults();
        string fingerprint = ModelFingerprint.Compute(model);
        path = Path.GetFullPath(path); result = Path.GetFullPath(result);
        bool verified = ResultProvenance.Verify(path, result, fingerprint);
        if (!verified && !allowUnverified) throw new InvalidDataException("Missing or mismatched analysis manifest. Run Model Cases and Combinations to generate verified results; Allow unverified is an explicit opt-in for externally solved files.");
        using var session = new StrausSession(path);
        int id = session.Id;
        var reference = new StringBuilder(1024);
        Check(St7.St7GetTitle(id, St7.tbReference, reference, reference.Capacity), "Read model identity");
        if (reference.ToString() != "Rhino2Straus:" + fingerprint) throw new InvalidDataException("The .st7 file does not belong to this Model. Export and solve the current Model.");
        var native = ValidateGeometry(id, model);
        var sections = new List<NativeBeamSection>();
        foreach (var group in model.FrameElements.Values.GroupBy(x => x.FrameProperty.Id))
        {
            int property = 0; Check(St7.St7GetElementProperty(id, St7.tyBEAM, native[(ResultEntity.Beam, group.First().Id)], ref property), "Read native beam property");
            var data = new double[11]; Check(St7.St7GetBeamSectionPropertyData(id, property, new int[1], data), "Read native section stiffness");
            if (data.Any(x => !double.IsFinite(x))) throw new InvalidDataException("Non-finite native section properties.");
            sections.Add(new(group.Key, property, data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10]));
        }
        string extension = Path.GetExtension(result).ToUpperInvariant();
        int hide = extension == ".NLA" ? St7.ipHideUnconvergedNLA : extension == ".NFA" ? St7.ipHideUnconvergedNFA : extension == ".LBA" ? St7.ipHideUnconvergedLBA : 0;
        if (hide != 0) Check(St7.St7SetResultFileOpenFlag(id, hide, St7.btTrue), "Hide unconverged results");
        var counts = session.OpenResults(result);
        var cases = new List<ResultCaseInfo>(); var records = new List<ElementResult>(); var issues = new List<ResultIssue>();
        var modal = new List<ModalResultInfo>();
        Check(St7.St7SetBeamResultPosMode(id, St7.bpLength), "Set station distances");
        for (int c = 1; c <= counts.Primary + counts.Secondary; c++)
        {
            var name = new StringBuilder(1024); Check(St7.St7GetResultCaseName(id, c, name, name.Capacity), "Read result case");
            double? frequency = null, buckling = null;
            if (extension == ".NFA") { var values = new double[16]; Check(St7.St7GetModalResultsNFA(id, c, values), "Read modal metadata"); modal.Add(new(c, values)); frequency = values[St7.ipFrequencyNFA]; }
            if (extension == ".LBA") { double factor = 0; Check(St7.St7GetBucklingFactor(id, c, ref factor), "Read buckling factor"); if (!double.IsFinite(factor)) throw new InvalidDataException("Non-finite buckling factor."); buckling = factor; }
            cases.Add(new(c, name.ToString(), c > counts.Primary, frequency, buckling));
            void Capture(ResultEntity entity, int entityId, ResultQuantity quantity, int surface, Func<ElementResult> fetch)
            {
                try { records.Add(fetch()); }
                catch (Exception e) when (e is InvalidOperationException || e is InvalidDataException || e is ArgumentException)
                { issues.Add(new(c, entity, entityId, quantity, surface, e.Message)); }
            }
            foreach (var node in model.NodeElements.Values)
            {
                int n = native[(ResultEntity.Node, node.Id)];
                foreach (bool reaction in new[] { false, true })
                {
                    var q = reaction ? ResultQuantity.Reaction : ResultQuantity.Displacement;
                    Capture(ResultEntity.Node, node.Id, q, 0, () => {
                        var values = new double[6]; Check(St7.St7GetNodeResult(id, reaction ? St7.rtNodeReact : St7.rtNodeDisp, n, c, values), "Read nodal " + q);
                        return new(c, ResultEntity.Node, node.Id, n, q, 0, "Global XYZ", "Node", reaction ? Reactions : Displacements, new[] { 0.0 }, new[] { values });
                    });
                }
            }
            ElementResult Beam(int n, int label, ResultEntity entity, ResultQuantity q, int nativeType, int axes, string[] columns)
            {
                int rows = 0, cols = 0; var pos = new double[St7.kMaxBeamResult]; var data = new double[St7.kMaxBeamResult];
                Check(St7.St7GetBeamResultArray(id, nativeType, axes, n, stations, c, ref rows, ref cols, pos, data), "Read beam " + q);
                return Make(c, entity, label, n, q, 0, axes == St7.stBeamPrincipal ? "Principal 1,2,3" : "Global XYZ", "Distance from end 1 in model length units", columns, pos.Take(rows), data, rows, cols);
            }
            foreach (var beam in model.FrameElements.Values)
            {
                int n = native[(ResultEntity.Beam, beam.Id)];
                foreach (var entry in new[] {
                    (ResultQuantity.Force, St7.rtBeamForce, St7.stBeamPrincipal, BeamForces),
                    (ResultQuantity.Stress, St7.rtBeamAllStress, St7.stBeamPrincipal, BeamStresses),
                    (ResultQuantity.Strain, St7.rtBeamAllStrain, St7.stBeamPrincipal, BeamStrains),
                    (ResultQuantity.TotalStrain, St7.rtBeamAllTotalStrain, St7.stBeamPrincipal, BeamStrains),
                    (ResultQuantity.Displacement, St7.rtBeamDisp, St7.stBeamGlobal, Displacements),
                    (ResultQuantity.NodeReaction, St7.rtBeamNodeReact, St7.stBeamGlobal, Reactions) })
                    Capture(ResultEntity.Beam, beam.Id, entry.Item1, 0, () => Beam(n, beam.Id, ResultEntity.Beam, entry.Item1, entry.Item2, entry.Item3, entry.Item4));
            }
            foreach (var plate in model.AreaElements.Values)
            {
                int n = native[(ResultEntity.Plate, plate.Id)];
                foreach (var entry in new[] { (ResultQuantity.Force, St7.rtPlateForce), (ResultQuantity.Moment, St7.rtPlateMoment),
                    (ResultQuantity.Stress, St7.rtPlateStress), (ResultQuantity.Strain, St7.rtPlateStrain),
                    (ResultQuantity.TotalStrain, St7.rtPlateTotalStrain), (ResultQuantity.Curvature, St7.rtPlateCurvature),
                    (ResultQuantity.NodeDisplacement, St7.rtPlateNodeDisp), (ResultQuantity.NodeReaction, St7.rtPlateNodeReact) })
                {
                    bool nodal = entry.Item1 is ResultQuantity.NodeDisplacement or ResultQuantity.NodeReaction;
                    bool throughThickness = entry.Item1 is ResultQuantity.Stress or ResultQuantity.Strain or ResultQuantity.TotalStrain;
                    foreach (int surface in throughThickness ? new[] { 0, 1, 2 } : new[] { 0 })
                        Capture(ResultEntity.Plate, plate.Id, entry.Item1, surface, () => {
                            int rows = 0, cols = 0; var data = new double[St7.kMaxPlateResult];
                            Check(St7.St7GetPlateResultArray(id, entry.Item2, nodal ? St7.stPlateGlobal : St7.stPlateLocal, n, c, sample, surface, 0, ref rows, ref cols, data), "Read plate " + entry.Item1);
                            return Make(c, ResultEntity.Plate, plate.Id, n, entry.Item1, surface, nodal ? "Global XYZ" : "Local x,y,z", Sampling(sample),
                                nodal ? (entry.Item1 == ResultQuantity.NodeReaction ? Reactions : Displacements) : Tensor, Enumerable.Range(0, rows).Select(x => (double)x), data, rows, cols);
                        });
                }
            }
            foreach (var solid in model.SolidElements.Values)
            {
                int n = native[(ResultEntity.Solid, solid.Id)];
                foreach (var entry in new[] { (ResultQuantity.Stress, St7.rtBrickStress), (ResultQuantity.Strain, St7.rtBrickStrain),
                    (ResultQuantity.TotalStrain, St7.rtBrickTotalStrain), (ResultQuantity.NodeDisplacement, St7.rtBrickNodeDisp), (ResultQuantity.NodeReaction, St7.rtBrickNodeReact) })
                    Capture(ResultEntity.Solid, solid.Id, entry.Item1, 0, () => {
                        int rows = 0, cols = 0; var data = new double[St7.kMaxBrickResult];
                        Check(St7.St7GetBrickResultArray(id, entry.Item2, St7.stBrickGlobal, n, c, sample, ref rows, ref cols, data), "Read solid " + entry.Item1);
                        return Make(c, ResultEntity.Solid, solid.Id, n, entry.Item1, 0, "Global XYZ", Sampling(sample),
                            entry.Item1 == ResultQuantity.NodeReaction ? Reactions : entry.Item1 == ResultQuantity.NodeDisplacement ? Displacements : Tensor,
                            Enumerable.Range(0, rows).Select(x => (double)x), data, rows, cols);
                    });
            }
            foreach (var link in model.LinkElements.Values)
            {
                bool isNativeLink = link.LinkProperty.IsNativeLink;
                int n = native[(ResultEntity.Link, link.Id)];
                foreach (bool reaction in new[] { false, true })
                {
                    var q = reaction ? ResultQuantity.NodeReaction : ResultQuantity.NodeDisplacement;
                    Capture(ResultEntity.Link, link.Id, q, 0, () => {
                        if (!isNativeLink) return Beam(n, link.Id, ResultEntity.Link, q, reaction ? St7.rtBeamNodeReact : St7.rtBeamDisp, St7.stBeamGlobal, reaction ? Reactions : Displacements);
                        int rows = 0, cols = 0; var data = new double[1024];
                        Check(St7.St7GetLinkResultArray(id, reaction ? St7.rtLinkNodeReact : St7.rtLinkNodeDisp, 1, n, c, ref rows, ref cols, data, data.Length), "Read native link " + q);
                        return Make(c, ResultEntity.Link, link.Id, n, q, 0, "Global XYZ", "Link node index", reaction ? Reactions : Displacements, Enumerable.Range(0, rows).Select(x => (double)x), data, rows, cols);
                    });
                }
            }
        }
        if (cases.Count == 0 || records.Count == 0) throw new InvalidDataException("No readable structural results: " + string.Join("; ", issues.Take(3).Select(i => i.Message)));
        model.AttachResults(new(fingerprint, path, result, Path.GetExtension(result).TrimStart('.'), verified, cases, records, issues, sections, modal));
        return model;
    }

    private static readonly string[] Displacements = { "UX", "UY", "UZ", "RX", "RY", "RZ" };
    private static readonly string[] Reactions = { "FX", "FY", "FZ", "MX", "MY", "MZ" };
    private static readonly string[] Tensor = { "XX", "YY", "ZZ", "XY", "YZ", "ZX" };
    private static readonly string[] BeamForces = { "V1", "M1", "V2", "M2", "N", "T" };
    private static readonly string[] BeamStrains = { "Axial", "Curvature1", "Curvature2", "Twist", "FibreMin", "FibreMax" };
    private static readonly string[] BeamStresses = { "FibreMin", "FibreMax", "Shear1Max", "Shear2Max", "Shear1Mean", "Shear2Mean", "ShearMagnitude", "PrincipalMin", "PrincipalMax", "HoopMin", "HoopMax", "AxialMin", "AxialMax", "Bending1Min", "Bending1Max", "Bending2Min", "Bending2Max", "YieldAreaRatio", "VonMises", "Tresca", "TorqueShear", "ShearF1", "ShearF2" };
    private static string Sampling(int mode) => mode switch { 0 => "Centroid", 1 => "Gauss points", 2 => "Nodes, no averaging", 3 => "Nodes, average all", 4 => "Nodes, average same property", _ => throw new ArgumentException() };
    private static ElementResult Make(int c, ResultEntity entity, int label, int n, ResultQuantity q, int surface, string axes, string sampling, string[] columns, IEnumerable<double> positions, double[] data, int rows, int cols)
    {
        if (rows < 1 || cols < 1) throw new InvalidDataException("Straus7 returned an empty table for this quantity.");
        var names = Enumerable.Range(0, cols).Select(i => i < columns.Length ? columns[i] : $"API[{i}]");
        return new(c, entity, label, n, q, surface, axes, sampling, names, positions, StrausFileService.Split(data, rows, cols));
    }

    private static Dictionary<(ResultEntity, int), int> ValidateGeometry(int id, ModelModel model)
    {
        var mapping = new Dictionary<(ResultEntity, int), int>();
        var units = new int[St7.kLastUnit]; Check(St7.St7GetUnits(id, units), "Read units");
        if (!units.SequenceEqual(Rhino2Straus.Core.Helper.StrausUnits.ToApi(model.ModelUnits))) throw new InvalidDataException("Model units differ from the native file.");
        int count = 0; Check(St7.St7GetTotal(id, St7.tyNODE, ref count), "Read node count");
        if (count != model.NodeElements.Count) throw new InvalidDataException("Node count differs from Model.");
        for (int n = 1; n <= count; n++)
        {
            int label = 0; var xyz = new double[3]; Check(St7.St7GetNodeID(id, n, ref label), "Read node ID"); Check(St7.St7GetNodeXYZ(id, n, xyz), "Read node position");
            if (!model.NodeElements.TryGetValue(label, out var node) || !mapping.TryAdd((ResultEntity.Node, label), n)) throw new InvalidDataException("Unknown or duplicate native node ID.");
            double tolerance = 1e-9 * Math.Max(1, Math.Max(Math.Abs(node.X), Math.Max(Math.Abs(node.Y), Math.Abs(node.Z))));
            if (label != node.Id || Math.Abs(xyz[0] - node.X) > tolerance || Math.Abs(xyz[1] - node.Y) > tolerance || Math.Abs(xyz[2] - node.Z) > tolerance)
                throw new InvalidDataException("Native node mapping differs from Model.");
        }
        foreach (var pair in new[] { (St7.tyBEAM, model.FrameElements.Count + model.LinkElements.Values.Count(x => !x.LinkProperty.IsNativeLink)),
            (St7.tyPLATE, model.AreaElements.Count), (St7.tyBRICK, model.SolidElements.Count),
            (St7.tyLINK, model.LinkElements.Values.Count(x => x.LinkProperty.IsNativeLink)) })
        {
            count = 0; Check(St7.St7GetTotal(id, pair.Item1, ref count), "Read element count");
            if (count != pair.Item2) throw new InvalidDataException("Native element count differs from Model.");
            for (int n = 1; n <= count; n++)
            {
                int label = 0;
                Check(pair.Item1 == St7.tyBEAM ? St7.St7GetBeamID(id, n, ref label) : pair.Item1 == St7.tyPLATE ? St7.St7GetPlateID(id, n, ref label) :
                    pair.Item1 == St7.tyBRICK ? St7.St7GetBrickID(id, n, ref label) : St7.St7GetLinkID(id, n, ref label), "Read element ID");
                var entity = pair.Item1 == St7.tyBEAM ? ResultEntity.Beam : pair.Item1 == St7.tyPLATE ? ResultEntity.Plate : pair.Item1 == St7.tyBRICK ? ResultEntity.Solid : ResultEntity.Link;
                Rhino2Straus.Core.Elements.NodeElementModel[] nodes;
                if (entity == ResultEntity.Beam && model.FrameElements.TryGetValue(label, out var beam)) nodes = new[] { beam.NodeStart, beam.NodeEnd };
                else if (entity == ResultEntity.Plate && model.AreaElements.TryGetValue(label, out var plate)) nodes = plate.NodeList.ToArray();
                else if (entity == ResultEntity.Solid && model.SolidElements.TryGetValue(label, out var solid)) nodes = solid.Nodes.ToArray();
                else
                {
                    if (entity == ResultEntity.Beam) { label -= model.FrameElements.Values.Select(x => x.Id).DefaultIfEmpty(0).Max(); entity = ResultEntity.Link; }
                    if (entity != ResultEntity.Link || !model.LinkElements.TryGetValue(label, out var link)) throw new InvalidDataException("Unknown native element ID.");
                    bool nativeLink = link.LinkProperty.IsNativeLink;
                    if (nativeLink != (pair.Item1 == St7.tyLINK)) throw new InvalidDataException("Link type differs from native file.");
                    nodes = new[] { link.NodeStart, link.NodeEnd };
                }
                if (!mapping.TryAdd((entity, label), n)) throw new InvalidDataException("Duplicate native element ID.");
                if (pair.Item1 != St7.tyLINK)
                {
                    var connection = new int[St7.kMaxElementNode + 1]; Check(St7.St7GetElementConnection(id, pair.Item1, n, connection), "Read element connectivity");
                    if (connection[0] != nodes.Length || !connection.Skip(1).Take(connection[0]).SequenceEqual(nodes.Select(x => mapping[(ResultEntity.Node, x.Id)])))
                        throw new InvalidDataException("Native element connectivity differs from Model.");
                }
                else
                {
                    NativeLinks.ValidateFile(id,n,model.LinkElements[label].LinkProperty,mapping[(ResultEntity.Node,nodes[0].Id)],mapping[(ResultEntity.Node,nodes[1].Id)]);
                }
            }
        }
        return mapping;
    }
}

public static partial class StrausFileService
{
    internal static string RunSelectedStatic(string path, bool overwrite, AnalysisSelection selection)
        => RunAnalysis(path, ".LSA", St7.stLinearStatic, overwrite, id => ConfigureStaticSelection(id, selection));

    internal static Task<string> RunSelectedStaticAsync(string path, bool overwrite, AnalysisSelection selection,
        CancellationToken cancellation = default)
        => RunAnalysisAsync(path, ".LSA", St7.stLinearStatic, overwrite, id => ConfigureStaticSelection(id, selection), cancellation);

    private static void ConfigureStaticSelection(int id, AnalysisSelection selection)
    {
            int count = 0; Check(St7.St7GetNumLoadCase(id, ref count), "Read load case count");
            for (int c = 1; c <= count; c++)
            {
                var name = new StringBuilder(1024); Check(St7.St7GetLoadCaseName(id, c, name, name.Capacity), "Read load case name");
                Check(selection.Cases.Contains(name.ToString()) ? St7.St7EnableLSALoadCase(id, c, 1) : St7.St7DisableLSALoadCase(id, c, 1), "Select load case");
            }
            count = 0; Check(St7.St7GetNumLSACombinations(id, ref count), "Read combinations");
            for (int c = 1; c <= count; c++)
            {
                var name = new StringBuilder(1024); Check(St7.St7GetLSACombinationName(id, c, name, name.Capacity), "Read combination name");
                Check(St7.St7SetLSACombinationState(id, c, (byte)(selection.Combinations.Contains(name.ToString()) ? St7.btTrue : St7.btFalse)), "Select combination");
            }
    }

    internal static AnalysisSelection ReadStaticSelection(string model, string results, int variableCase, int fixedCase)
    {
        using var session = new StrausSession(model);
        int id = session.Id; var counts = session.OpenResults(results);
        if (variableCase > counts.Primary + counts.Secondary || fixedCase > counts.Primary + counts.Secondary) throw new ArgumentException("Initial result case is outside the static result file range.");
        var cases = new List<string>(); var combinations = new List<string>(); int count = 0;
        Check(St7.St7GetNumLoadCase(id, ref count), "Read static cases");
        for (int c = 1; c <= count; c++)
        {
            byte enabled = 0; Check(St7.St7GetLSALoadCaseState(id, c, 1, ref enabled), "Read active static case");
            if (enabled == 0) continue; var name = new StringBuilder(1024); Check(St7.St7GetLoadCaseName(id, c, name, name.Capacity), "Read static case name"); cases.Add(name.ToString());
        }
        Check(St7.St7GetNumLSACombinations(id, ref count), "Read static combinations");
        for (int c = 1; c <= count; c++)
        {
            byte enabled = 0; Check(St7.St7GetLSACombinationState(id, c, ref enabled), "Read active static combination");
            if (enabled == 0) continue; var name = new StringBuilder(1024); Check(St7.St7GetLSACombinationName(id, c, name, name.Capacity), "Read static combination name"); combinations.Add(name.ToString());
        }
        return new(cases.ToArray(), combinations.ToArray());
    }
}
