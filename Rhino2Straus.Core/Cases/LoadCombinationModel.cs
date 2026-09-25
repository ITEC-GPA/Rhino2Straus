using System.Collections.Generic;
using Rhino2Straus.Core.Base;

namespace Rhino2Straus.Core.Cases
{
    public class LoadCombinationModel : LoadCaseBase
    {
        public enum LoadCombinationTypes
        {
            Linear,
            Envelope,
        }

        public List<LoadFactorModel> LoadFactorList { get; set; }

        public LoadCombinationTypes Type { get; set; }

        public LoadCombinationModel(string name, List<LoadFactorModel> loadFactorList, LoadCombinationTypes type, string description = "")
            : base(name, description)
        {
            LoadFactorList = loadFactorList;
            Type = type;
            Description = description;
        }

        public LoadCombinationModel()
            :base()
        {
        }

        public LoadCombinationModel(LoadCombinationModel loadCombinationModel)
            : base(loadCombinationModel.Name, loadCombinationModel.Description)
        {
            Id = loadCombinationModel.Id;
            Type = loadCombinationModel.Type;
            LoadFactorList = new List<LoadFactorModel>();
            for (int i = 0; i < loadCombinationModel.LoadFactorList.Count; i++)
                LoadFactorList.Add(new LoadFactorModel(loadCombinationModel.LoadFactorList[i]));
        }
    }
}
