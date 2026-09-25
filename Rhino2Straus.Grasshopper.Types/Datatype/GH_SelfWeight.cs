using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.Cases;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_SelfWeight : GH_Goo<SelfWeightModel>
    {
        public GH_SelfWeight(SelfWeightModel model)
        {
            Value = model;
        }

        public GH_SelfWeight()
        {

        }

        public override bool IsValid => Value != null;

        public override string TypeName => "SelfWeightModel";

        public override string TypeDescription => "SelfWeightModel";

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new GH_SelfWeight() : new GH_SelfWeight(new SelfWeightModel(Value));
        }

        public override string ToString()
        {
            return "Load case name: " + Value.LoadCase.Name + ", \r" +
                "FactorX: " + Value.FactorX + "FactorY: " + Value.FactorY + "FactorZ: " + Value.FactorZ;
        }
    }
}
