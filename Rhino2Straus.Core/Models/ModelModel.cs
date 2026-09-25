using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Base;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.Collections;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhino2Straus.Core.Models
{
    public class ModelModel : IEquatable<ModelModel>
    {
        public ModelUnitsModel ModelUnits { get; set; } = new ModelUnitsModel();
        public UniqueIdCollection<MaterialModel> Materials { get; }
        public UniqueNameCollection<LoadCaseModel> LoadCases { get; }
        public UniqueNameCollection<LoadCombinationModel> LoadCombinations { get; }
        public UniqueNameCollection<ElementGroupModel> Groups { get; }
        public UniqueIdCollection<FrameSectionModel> FrameSections { get; }
        public UniqueIdCollection<FramePropertyModel> FrameProperties { get; }
        public UniqueIdCollection<AreaThicknessModel> AreaThicknesses { get; }
        public UniqueIdCollection<AreaPropertyModel> AreaProperties { get; }
        public UniqueIdCollection<NodeElementModel> NodeElements { get; }
        public UniqueIdCollection<FrameElementModel> FrameElements { get; }
        public UniqueIdCollection<AreaElementModel> AreaElements { get; }
        public UniqueIdCollection<LinkElementModel> LinkElements { get; }
        public UniqueIdCollection<SolidElementModel> SolidElements { get; } = new();
        public SelfWeightModel SelfWeight { get; set; }
        public AnalysisResultsModel Results { get; private set; }
        public void ClearResults() => Results = null;
        public void AttachResults(AnalysisResultsModel results)
        {
            ArgumentNullException.ThrowIfNull(results);
            results.ValidateFor(this);
            Results = results;
        }

        public ModelModel()
        {
            Materials = new UniqueIdCollection<MaterialModel>();
            LoadCases = new UniqueNameCollection<LoadCaseModel>();
            LoadCombinations = new UniqueNameCollection<LoadCombinationModel>();
            Groups = new UniqueNameCollection<ElementGroupModel>();
            FrameSections = new UniqueIdCollection<FrameSectionModel>();
            FrameProperties = new UniqueIdCollection<FramePropertyModel>();
            AreaThicknesses = new UniqueIdCollection<AreaThicknessModel>();
            AreaProperties = new UniqueIdCollection<AreaPropertyModel>();
            NodeElements = new UniqueIdCollection<NodeElementModel>();
            FrameElements = new UniqueIdCollection<FrameElementModel>();
            AreaElements = new UniqueIdCollection<AreaElementModel>();
            LinkElements = new UniqueIdCollection<LinkElementModel>();
        }

        public ModelModel(ModelModel model) : this()
        {
            ModelUnits = new ModelUnitsModel(model.ModelUnits);
            BuildModel(model.NodeElements.Values.ToList(), model.FrameElements.Values.ToList(), model.AreaElements.Values.ToList(), model.LinkElements.Values.ToList(), model.LoadCombinations.Values.ToList(), model.SelfWeight, model.SolidElements.Values.ToList());
            // Preserve explicitly named cases and their metadata/order, including unused cases.
            LoadCases.Clear(); LoadCases.AddRange(model.LoadCases.Values.Select(x => new LoadCaseModel(x)));
            ModelBuilder.CanonicalizeCases(this);
            foreach (var group in Groups) if (model.Groups.TryGetValue(group.Key, out var original)) group.Value.Id = original.Id;
            Results = model.Results;
        }

        public void BuildModel(List<NodeElementModel> inputNodes = null, List<FrameElementModel> inputFrames = null,
            List<AreaElementModel> inputAreas = null, List<LinkElementModel> inputLinks = null,
            List<LoadCombinationModel> inputcombos = null, SelfWeightModel inputSelfWeight = null, List<SolidElementModel> inputSolids = null)
        {
            var built = new ModelModel { ModelUnits = new ModelUnitsModel(ModelUnits) };
            ModelBuilder.Build(built, inputNodes, inputFrames, inputAreas, inputLinks, inputcombos, inputSelfWeight, inputSolids);
            Materials.Clear(); Materials.AddRange(built.Materials.Values);
            FrameSections.Clear(); FrameSections.AddRange(built.FrameSections.Values);
            FrameProperties.Clear(); FrameProperties.AddRange(built.FrameProperties.Values);
            AreaThicknesses.Clear(); AreaThicknesses.AddRange(built.AreaThicknesses.Values);
            AreaProperties.Clear(); AreaProperties.AddRange(built.AreaProperties.Values);
            NodeElements.Clear(); NodeElements.AddRange(built.NodeElements.Values);
            FrameElements.Clear(); FrameElements.AddRange(built.FrameElements.Values);
            AreaElements.Clear(); AreaElements.AddRange(built.AreaElements.Values);
            LinkElements.Clear(); LinkElements.AddRange(built.LinkElements.Values);
            LoadCases.Clear(); LoadCases.AddRange(built.LoadCases.Values);
            LoadCombinations.Clear(); LoadCombinations.AddRange(built.LoadCombinations.Values);
            Groups.Clear(); foreach (var group in built.Groups) Groups.Add(group.Key, group.Value);
            SolidElements.Clear(); SolidElements.AddRange(built.SolidElements.Values);
            SelfWeight = built.SelfWeight;
            Results = null;
        }

        public bool CreateModelStraus7R3(string outputPath, out int modelId, out List<string> warnings, out List<string> errors, bool overwrite = false)
        {
            HelperStrausExport helperStrausExport = new HelperStrausExport(this);
            if (helperStrausExport.CreateModelR3(outputPath, out modelId, out warnings, out errors, overwrite))
                return true;
            return false;
        }

        #region Overrides and Operators

        public override bool Equals(object obj)
        {
            return Equals(obj as ModelModel);
        }

        public bool Equals(ModelModel other)
        {
            return !(other is null) &&
                   EqualityComparer<ModelUnitsModel>.Default.Equals(ModelUnits, other.ModelUnits) &&
                   EqualityComparer<UniqueIdCollection<MaterialModel>>.Default.Equals(Materials, other.Materials) &&
                   EqualityComparer<UniqueNameCollection<LoadCaseModel>>.Default.Equals(LoadCases, other.LoadCases) &&
                   EqualityComparer<UniqueNameCollection<LoadCombinationModel>>.Default.Equals(LoadCombinations, other.LoadCombinations) &&
                   EqualityComparer<UniqueNameCollection<ElementGroupModel>>.Default.Equals(Groups, other.Groups) &&
                   EqualityComparer<UniqueIdCollection<FrameSectionModel>>.Default.Equals(FrameSections, other.FrameSections) &&
                   EqualityComparer<UniqueIdCollection<AreaThicknessModel>>.Default.Equals(AreaThicknesses, other.AreaThicknesses) &&
                   EqualityComparer<UniqueIdCollection<NodeElementModel>>.Default.Equals(NodeElements, other.NodeElements) &&
                   EqualityComparer<UniqueIdCollection<FrameElementModel>>.Default.Equals(FrameElements, other.FrameElements) &&
                   EqualityComparer<UniqueIdCollection<AreaElementModel>>.Default.Equals(AreaElements, other.AreaElements);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + EqualityComparer<ModelUnitsModel>.Default.GetHashCode(ModelUnits);
            hashCode = hashCode * -17 + EqualityComparer<UniqueIdCollection<MaterialModel>>.Default.GetHashCode(Materials);
            hashCode = hashCode * -17 + EqualityComparer<UniqueNameCollection<LoadCaseModel>>.Default.GetHashCode(LoadCases);
            hashCode = hashCode * -17 + EqualityComparer<UniqueNameCollection<LoadCombinationModel>>.Default.GetHashCode(LoadCombinations);
            hashCode = hashCode * -17 + EqualityComparer<UniqueNameCollection<ElementGroupModel>>.Default.GetHashCode(Groups);
            hashCode = hashCode * -17 + EqualityComparer<UniqueIdCollection<FrameSectionModel>>.Default.GetHashCode(FrameSections);
            hashCode = hashCode * -17 + EqualityComparer<UniqueIdCollection<AreaThicknessModel>>.Default.GetHashCode(AreaThicknesses);
            hashCode = hashCode * -17 + EqualityComparer<UniqueIdCollection<NodeElementModel>>.Default.GetHashCode(NodeElements);
            hashCode = hashCode * -17 + EqualityComparer<UniqueIdCollection<FrameElementModel>>.Default.GetHashCode(FrameElements);
            hashCode = hashCode * -17 + EqualityComparer<UniqueIdCollection<AreaElementModel>>.Default.GetHashCode(AreaElements);
            return hashCode;
        }

        public static bool operator ==(ModelModel left, ModelModel right)
        {
            return EqualityComparer<ModelModel>.Default.Equals(left, right);
        }

        public static bool operator !=(ModelModel left, ModelModel right)
        {
            return !(left == right);
        }

        #endregion
    }
}
