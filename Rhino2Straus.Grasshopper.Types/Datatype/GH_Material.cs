using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.ElementProperties;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_Material : GH_Goo<MaterialModel>
    {
        public GH_Material(MaterialModel model)
        {
            Value = model;
        }

        public GH_Material()
        {

        }

        public override bool IsValid => Value != null;

        public override string TypeName => "v";

        public override string TypeDescription => "MaterialModel";

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new GH_Material() : new GH_Material(new MaterialModel(Value));
        }

        public override string ToString()
        {
            return "Name: " + Value.Name + ", \r" +
                "Type: " + Value.Type;
        }
    }
}
