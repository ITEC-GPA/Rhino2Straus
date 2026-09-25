using Rhino2Straus.Core.Attributes;
using System.Collections.Generic;

namespace Rhino2Straus.Core.Base
{
    public abstract class ElementModel : ModelObjectId
    {
        public List<ElementGroupModel> Groups { get; set; }

        public ElementModel(int id)
            :base()
        {
            Id = id;
        }

        public ElementModel()
            :base()
        {
            Id = UNASSIGNED;
            Groups = new List<ElementGroupModel>();
        }

        public ElementModel(ElementModel elementModel)
            : base(elementModel.Id, elementModel.Name)
        {
            Groups = new List<ElementGroupModel>(elementModel.Groups);
        }

        public virtual void Merge(ElementModel other)
        {
            Groups.AddRange(other.Groups);
        }
    }
}
