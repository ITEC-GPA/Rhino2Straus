using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.Cases;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_LoadFactor : GH_Goo<LoadFactorModel>
    {
        public GH_LoadFactor(LoadFactorModel model)
        {
            Value = model;
        }

        public GH_LoadFactor()
        {

        }

        public override bool IsValid => Value != null;

        public override string TypeName => "LoadFactorModel";

        public override string TypeDescription => "LoadFactorModel";

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new GH_LoadFactor() : new GH_LoadFactor(new LoadFactorModel(Value));
        }

        public override string ToString()
        {
            return "Load Factor: " + Value.LoadCase.Name + " - " + Value.Factor;
        }
    }
}
