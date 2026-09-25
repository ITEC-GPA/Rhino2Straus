using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.Cases;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_LoadCase : GH_Goo<LoadCaseModel>
    {
        public GH_LoadCase(LoadCaseModel model)
        {
            Value = model;
        }

        public GH_LoadCase()
        {

        }

        public override bool IsValid => Value != null;

        public override string TypeName => "LoadCaseModel";

        public override string TypeDescription => "LoadCaseModel";

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new GH_LoadCase() : new GH_LoadCase(new LoadCaseModel(Value));
        }

        public override string ToString()
        {
            return "Load case name: " + Value.Name;
        }
    }
}
