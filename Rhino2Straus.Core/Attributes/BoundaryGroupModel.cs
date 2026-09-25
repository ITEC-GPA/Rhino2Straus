using Rhino2Straus.Core.Base;

namespace Rhino2Straus.Core.Attributes
{
    public class BoundaryGroupModel : ModelObjectId
    {
        public BoundaryGroupModel(string name)
            : base(name)
        {
        }

        public BoundaryGroupModel()
            : base()
        {
        }

        public BoundaryGroupModel(BoundaryGroupModel elementGroupModel)
            : base(elementGroupModel.Id, elementGroupModel.Name)
        {
        }
    }
}
