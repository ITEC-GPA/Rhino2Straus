using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.Attributes;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_LoadGroup : GH_Goo<LoadGroupModel>
    {
        public GH_LoadGroup(LoadGroupModel model)
        {
            Value = model;
        }

        public GH_LoadGroup()
        {

        }

        public override bool IsValid => Value != null;

        public override string TypeName => "LoadGroupModel";

        public override string TypeDescription => "LoadGroupModel";

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new GH_LoadGroup() : new GH_LoadGroup(new LoadGroupModel(Value));
        }

        public override string ToString()
        {
            return "Name: " + Value.Name;
        }
    }
}
