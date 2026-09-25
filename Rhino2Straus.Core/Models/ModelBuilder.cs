using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Base;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.Collections;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.Loads;

namespace Rhino2Straus.Core.Models;

internal static class ModelBuilder
{
    public static void Build(ModelModel m, List<NodeElementModel> nodes, List<FrameElementModel> beams,
        List<AreaElementModel> plates, List<LinkElementModel> links, List<LoadCombinationModel> combinations, SelfWeightModel weight, List<SolidElementModel> solids)
    {
        solids ??= new(); nodes ??= new(); beams ??= new(); plates ??= new(); links ??= new(); combinations ??= new();
        double tolerance = m.ModelUnits.Tolerance;
        if (!double.IsFinite(tolerance) || tolerance <= 0) throw new ArgumentException("Node tolerance must be finite and positive.");
        var allNodes = nodes.Concat(beams.SelectMany(b => new[] { b.NodeStart, b.NodeEnd }))
            .Concat(solids.SelectMany(s=>s.Nodes)).Concat(plates.SelectMany(p => p.NodeList)).Concat(links.SelectMany(l => new[] { l.NodeStart, l.NodeEnd })).ToList();
        if (allNodes.Any(n => n == null || !n.Position.IsValid || n.Id == 0 || n.Id < -1)) throw new ArgumentException("Invalid node coordinates or ID. Use positive IDs or -1.");
        var reserved = allNodes.Where(n => n.Id > 0).Select(n => n.Id).ToHashSet();
        var sources = new Dictionary<NodeElementModel, NodeElementModel>(ReferenceEqualityComparer.Instance);
        var byInputId = new Dictionary<int, NodeElementModel>();
        var grid = new Dictionary<(long, long, long), List<NodeElementModel>>();
        (long, long, long) Cell(Point3d p) => (checked((long)Math.Floor(p.X / tolerance)), checked((long)Math.Floor(p.Y / tolerance)), checked((long)Math.Floor(p.Z / tolerance)));
        int nextNode = 1;
        List<ElementGroupModel> Groups(IEnumerable<ElementGroupModel> groups)
        {
            var result = new List<ElementGroupModel>();
            foreach (var source in groups ?? Enumerable.Empty<ElementGroupModel>())
            {
                if (source == null || string.IsNullOrWhiteSpace(source.Name)) throw new ArgumentException("Group names must not be empty.");
                string key = source.GroupFullName;
                if (!m.Groups.TryGetValue(key, out var group)) { group = new ElementGroupModel(source) { Id = m.Groups.Count + 1 }; m.Groups.Add(key, group); }
                if (!result.Contains(group)) result.Add(group);
            }
            return result;
        }
        NodeElementModel Resolve(NodeElementModel source)
        {
            if (sources.TryGetValue(source, out var known)) return known;
            NodeElementModel target = null;
            if (source.Id > 0 && byInputId.TryGetValue(source.Id, out target) && target.Position.DistanceTo(source.Position) > tolerance)
                throw new ArgumentException($"Node ID {source.Id} occurs at different positions.");
            var cell = Cell(source.Position);
            if (target == null)
            {
                var nearby = new List<NodeElementModel>();
                for (int x = -1; x <= 1; x++) for (int y = -1; y <= 1; y++) for (int z = -1; z <= 1; z++)
                    if (grid.TryGetValue((cell.Item1 + x, cell.Item2 + y, cell.Item3 + z), out var list)) nearby.AddRange(list);
                target = nearby.Where(n => n.Position.DistanceTo(source.Position) <= tolerance).OrderBy(n => n.Position.DistanceTo(source.Position)).ThenBy(n => n.Id).FirstOrDefault();
            }
            if (target == null)
            {
                target = new NodeElementModel(source) { Groups = Groups(source.Groups) };
                if (target.Id < 1) { while (reserved.Contains(nextNode)) nextNode++; target.Id = nextNode++; reserved.Add(target.Id); }
                m.NodeElements.Add(target);
                if (!grid.TryGetValue(cell, out var list)) grid[cell] = list = new();
                list.Add(target);
            }
            else
            {
                var a = target.Support; var b = source.Support;
                var sa=new[]{a.Dx,a.Dy,a.Dz,a.Mx,a.My,a.Mz};var sb=new[]{b.Dx,b.Dy,b.Dz,b.Mx,b.My,b.Mz};
                for(int dof=0;dof<6;dof++)
                {
                    if(sa[dof]&&sb[dof]&&target.PrescribedDisplacement[dof]!=source.PrescribedDisplacement[dof])throw new ArgumentException($"Conflicting imposed displacement at node {source.Id}, DOF {dof+1}.");
                    if(sb[dof])target.PrescribedDisplacement[dof]=source.PrescribedDisplacement[dof];
                }
                a.Dx |= b.Dx; a.Dy |= b.Dy; a.Dz |= b.Dz; a.Mx |= b.Mx; a.My |= b.My; a.Mz |= b.Mz;
                target.NodalLoadList.AddRange(source.NodalLoadList.Select(x => new NodalLoadModel(x)));
                target.Temperatures.AddRange(source.Temperatures.Select(x => new NodeTemperatureModel { LoadCase = x.LoadCase, Temperature = x.Temperature }));
                target.TranslationMass += source.TranslationMass; target.RotationMass += source.RotationMass;
                target.TranslationSpring += source.TranslationSpring; target.RotationSpring += source.RotationSpring;
                target.Groups = Groups(target.Groups.Concat(source.Groups));
            }
            sources[source] = target;
            if (source.Id > 0) byInputId[source.Id] = target;
            return target;
        }
        // Explicit nodes are authoritative and keep their requested numbering where possible.
        foreach (var n in nodes.OrderBy(n => n.Id < 1 ? int.MaxValue : n.Id)) Resolve(n);

        T Register<T>(UniqueIdCollection<T> collection, T value) where T : ModelObjectId
        {
            var same = collection.Values.FirstOrDefault(x => EqualityComparer<T>.Default.Equals(x, value));
            if (same != null) return same;
            if (value.Id > 0 && collection.ContainsKey(value.Id)) throw new ArgumentException($"Conflicting {typeof(T).Name} ID {value.Id}.");
            collection.Add(value); return value;
        }
        void AddElement<T>(UniqueIdCollection<T> collection, T value, HashSet<int> ids) where T : ModelObjectId
        {
            if (value.Id == 0 || value.Id < -1) throw new ArgumentException("Use positive element IDs or -1.");
            if (value.Id < 1) { int id = 1; while (ids.Contains(id)) id++; value.Id = id; ids.Add(id); }
            if (collection.ContainsKey(value.Id)) throw new ArgumentException($"Duplicate {typeof(T).Name} ID {value.Id}.");
            collection.Add(value);
        }
        var beamIds = beams.Where(b => b.Id > 0).Select(b => b.Id).ToHashSet();
        foreach (var input in beams)
        {
            if (input.FrameProperty == null || input.Material == null || input.FrameSection == null) throw new ArgumentException("Beam needs a material and section.");
            var b = new FrameElementModel(input) { NodeStart = Resolve(input.NodeStart), NodeEnd = Resolve(input.NodeEnd), Groups = Groups(input.Groups) };
            b.Material = Register(m.Materials, new MaterialModel(input.Material));
            b.FrameSection = Register(m.FrameSections, new FrameSectionModel(input.FrameSection));
            b.FrameProperty = Register(m.FrameProperties, new FramePropertyModel(input.FrameProperty) { Material = b.Material, Section = b.FrameSection });
            if (b.Id > 0 && m.FrameElements.TryGetValue(b.Id, out var previous))
            {
                if (previous.NodeStart.Id != b.NodeStart.Id || previous.NodeEnd.Id != b.NodeEnd.Id || !previous.FrameProperty.Equals(b.FrameProperty) || previous.Angle != b.Angle || previous.Offset != b.Offset || !previous.StartReleases.SequenceEqual(b.StartReleases) || !previous.EndReleases.SequenceEqual(b.EndReleases))
                    throw new ArgumentException($"Conflicting geometry or properties for beam ID {b.Id}.");
                previous.FrameLoadList.AddRange(b.FrameLoadList); previous.Groups = Groups(previous.Groups.Concat(b.Groups));
            }
            else AddElement(m.FrameElements, b, beamIds);
        }
        var plateIds = plates.Where(p => p.Id > 0).Select(p => p.Id).ToHashSet();
        foreach (var input in plates)
        {
            if (input.AreaProperty == null || input.Material == null || input.AreaThickness == null) throw new ArgumentException("Plate needs a material and thickness.");
            var p = new AreaElementModel(input) { NodeList = input.NodeList.Select(Resolve).ToList(), Groups = Groups(input.Groups) };
            p.Material = Register(m.Materials, new MaterialModel(input.Material));
            p.AreaThickness = Register(m.AreaThicknesses, new AreaThicknessModel(input.AreaThickness));
            p.AreaProperty = Register(m.AreaProperties, new AreaPropertyModel(input.AreaProperty) { Material = p.Material, Section = p.AreaThickness });
            if (p.Id > 0 && m.AreaElements.TryGetValue(p.Id, out var previous))
            {
                if (!previous.NodeList.Select(n=>n.Id).SequenceEqual(p.NodeList.Select(n=>n.Id)) || !previous.AreaProperty.Equals(p.AreaProperty) || previous.Angle != p.Angle || previous.Offset != p.Offset)
                    throw new ArgumentException($"Conflicting geometry or properties for plate ID {p.Id}.");
                previous.AreaLoadList.AddRange(p.AreaLoadList); previous.Groups = Groups(previous.Groups.Concat(p.Groups));
            }
            else AddElement(m.AreaElements, p, plateIds);
        }
        var solidIds=solids.Where(s=>s.Id>0).Select(s=>s.Id).ToHashSet();
        foreach(var input in solids)
        {
            if(input.Material==null)throw new ArgumentException("Solid needs a material.");
            var s=new SolidElementModel(input){Nodes=input.Nodes.Select(Resolve).ToList(),Groups=Groups(input.Groups),Material=Register(m.Materials,new MaterialModel(input.Material))};
            if(s.Id>0&&m.SolidElements.TryGetValue(s.Id,out var previous))
            {
                if(!s.Nodes.Select(n=>n.Id).SequenceEqual(previous.Nodes.Select(n=>n.Id))||!s.Material.Equals(previous.Material))throw new ArgumentException($"Conflicting solid ID {s.Id}.");
                previous.Pressures.AddRange(s.Pressures);previous.Groups=Groups(previous.Groups.Concat(s.Groups));
            }
            else AddElement(m.SolidElements,s,solidIds);
        }
        var linkIds = links.Where(l => l.Id > 0).Select(l => l.Id).ToHashSet();
        foreach (var input in links)
            AddElement(m.LinkElements, new LinkElementModel(input) { NodeStart = Resolve(input.NodeStart), NodeEnd = Resolve(input.NodeEnd), Groups = Groups(input.Groups), LinkProperty = new LinkPropertyModel(input.LinkProperty) }, linkIds);
        void Case(LoadCaseModel c)
        {
            if (c == null || string.IsNullOrWhiteSpace(c.Name)) throw new ArgumentException("Every load needs a named load case.");
            if (!m.LoadCases.ContainsKey(c.Name)) m.LoadCases.Add(new LoadCaseModel(c) { Id = m.LoadCases.Count + 1 });
            else if (m.LoadCases[c.Name].Type != c.Type || m.LoadCases[c.Name].GravityNoStructuralMass != c.GravityNoStructuralMass)
                throw new ArgumentException($"Conflicting settings for load case {c.Name}.");
        }
        if (weight != null) { m.SelfWeight = new SelfWeightModel(weight); Case(weight.LoadCase); }
        foreach (var n in m.NodeElements.Values) { foreach (var l in n.NodalLoadList) Case(l.LoadCase); foreach (var l in n.Temperatures) Case(l.LoadCase); }
        foreach (var b in m.FrameElements.Values) foreach (var l in b.FrameLoadList) Case(l.LoadCase);
        foreach (var p in m.AreaElements.Values) foreach (var l in p.AreaLoadList) Case(l.LoadCase);
        foreach(var solid in m.SolidElements.Values)foreach(var p in solid.Pressures)Case(p.LoadCase);
        foreach (var c in combinations)
        {
            if (string.IsNullOrWhiteSpace(c.Name) || m.LoadCombinations.ContainsKey(c.Name)) throw new ArgumentException("Combination names must be nonempty and unique.");
            var combo = new LoadCombinationModel(c) { Id = m.LoadCombinations.Count + 1 };
            foreach (var factor in combo.LoadFactorList) { Case(factor.LoadCase); factor.LoadCase = m.LoadCases[factor.LoadCase.Name]; }
            m.LoadCombinations.Add(combo);
        }
        CanonicalizeCases(m);
    }

    internal static void CanonicalizeCases(ModelModel m)
    {
        LoadCaseModel Case(LoadCaseModel c) => m.LoadCases[c.Name];
        if (m.SelfWeight != null) m.SelfWeight.LoadCase = Case(m.SelfWeight.LoadCase);
        foreach (var n in m.NodeElements.Values)
        { foreach (var l in n.NodalLoadList) l.LoadCase = Case(l.LoadCase); foreach (var l in n.Temperatures) l.LoadCase = Case(l.LoadCase); }
        foreach (var b in m.FrameElements.Values) foreach (var l in b.FrameLoadList) l.LoadCase = Case(l.LoadCase);
        foreach (var p in m.AreaElements.Values) foreach (var l in p.AreaLoadList) l.LoadCase = Case(l.LoadCase);
        foreach (var s in m.SolidElements.Values) s.Pressures = s.Pressures.Select(p => p with { LoadCase = Case(p.LoadCase) }).ToList();
        foreach (var c in m.LoadCombinations.Values) foreach (var f in c.LoadFactorList) f.LoadCase = Case(f.LoadCase);
    }
}
