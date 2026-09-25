using Rhino2Straus.Core.Base;

namespace Rhino2Straus.Core.Attributes
{
    public class LoadGroupModel : ModelObjectId
    {
        public string Suffix { get; set; }

        public LoadGroupModel(string name, string suffix = "")
            : base(name)
        {
            Suffix = suffix;
        }

        public LoadGroupModel()
            : base()
        {
        }

        public LoadGroupModel(LoadGroupModel elementGroupModel)
            : base(elementGroupModel.Id, elementGroupModel.Name)
        {
            Suffix = elementGroupModel.Suffix;
        }
    }
}
