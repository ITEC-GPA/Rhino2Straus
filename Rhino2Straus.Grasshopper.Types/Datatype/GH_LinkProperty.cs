using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.ElementProperties;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_LinkProperty : GH_Goo<LinkPropertyModel>
    {
        public GH_LinkProperty(LinkPropertyModel model)
        {
            Value = model;
        }

        public GH_LinkProperty()
        {

        }

        public override bool IsValid => Value != null;

        public override string TypeName => "LinkPropertyModel";

        public override string TypeDescription => "LinkPropertyModel";

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new GH_LinkProperty() : new GH_LinkProperty(new LinkPropertyModel(Value));
        }

        public override string ToString()
        {
            return "Name: " + Value.Name;
        }
    }
}
