using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.Cases;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_LoadCombination : GH_Goo<LoadCombinationModel>
    {
        public GH_LoadCombination(LoadCombinationModel model)
        {
            Value = model;
        }

        public GH_LoadCombination()
        {

        }

        public override bool IsValid => Value != null;

        public override string TypeName => "LoadCombinationModel";

        public override string TypeDescription => "LoadCombinationModel";

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new GH_LoadCombination() : new GH_LoadCombination(new LoadCombinationModel(Value));
        }

        public override string ToString()
        {
            return "Load combination name: " + Value.Name;
        }
    }
}
