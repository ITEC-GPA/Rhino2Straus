using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.ElementProperties;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_AreaProperty : GH_Goo<AreaPropertyModel>
    {
        public GH_AreaProperty(AreaPropertyModel model)
        {
            Value = model;
        }

        public GH_AreaProperty()
        {

        }

        public override bool IsValid => Value != null;

        public override string TypeName => "AreaPropertyModel";

        public override string TypeDescription => "AreaPropertyModel";

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new GH_AreaProperty() : new GH_AreaProperty(new AreaPropertyModel(Value));
        }

        public override string ToString()
        {
            return "Name: " + Value.Name + "\n" +
                "Thickness: " + Value.ThicknessMembrane;
        }
    }
}
