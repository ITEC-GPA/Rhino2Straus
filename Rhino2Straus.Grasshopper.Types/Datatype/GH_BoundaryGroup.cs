using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.Attributes;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_BoundaryGroup : GH_Goo<BoundaryGroupModel>
    {
        public GH_BoundaryGroup(BoundaryGroupModel model)
        {
            Value = model;
        }

        public GH_BoundaryGroup()
        {

        }

        public override bool IsValid => Value != null;

        public override string TypeName => "BoundaryGroupModel";

        public override string TypeDescription => "BoundaryGroupModel";

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new GH_BoundaryGroup() : new GH_BoundaryGroup(new BoundaryGroupModel(Value));
        }

        public override string ToString()
        {
            return "Name: " + Value.Name;
        }
    }
}
