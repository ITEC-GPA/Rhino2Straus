using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.ElementProperties;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_AreaThickness : GH_Goo<AreaThicknessModel>
    {
        public GH_AreaThickness(AreaThicknessModel model)
        {
            Value = model;
        }

        public GH_AreaThickness()
        {

        }

        public override bool IsValid => Value != null;

        public override string TypeName => "AreaThicknessModel";

        public override string TypeDescription => "AreaThicknessModel";

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new GH_AreaThickness() : new GH_AreaThickness(new AreaThicknessModel(Value));
        }

        public override string ToString()
        {
            return "Name: " + Value.Name + "\n" +
                "Thickness: " + Value.ThicknessMembrane;
        }
    }
}
