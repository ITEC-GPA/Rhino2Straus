using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.Attributes;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_ElementGroup : GH_Goo<ElementGroupModel>
    {
        public GH_ElementGroup(ElementGroupModel model)
        {
            Value = model;
        }

        public GH_ElementGroup()
        {

        }

        public override bool IsValid => Value != null;

        public override string TypeName => "ElementGroupModel";

        public override string TypeDescription => "ElementGroupModel";

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new GH_ElementGroup() : new GH_ElementGroup(new ElementGroupModel(Value));
        }

        public override string ToString()
        {
            return "Name: " + Value.Name;
        }
    }
}
