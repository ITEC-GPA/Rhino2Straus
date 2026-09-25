using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.Settings;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_Units : GH_Goo<ModelUnitsModel>
    {
        public GH_Units(ModelUnitsModel model)
        {
            Value = model;
        }

        public GH_Units()
        {

        }

        public override bool IsValid => Value != null;

        public override string TypeName => "ModelUnits";

        public override string TypeDescription => "ModelUnits";

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new GH_Units() : new GH_Units(new ModelUnitsModel(Value));
        }

        public override string ToString()
        {
            return "Units: " + Value.ForceUnit.ToString() + "; " + Value.LengthUnit.ToString() + "; " + Value.TemperatureUnit.ToString() + "; " + Value.HeatUnit.ToString() + "; " + Value.Tolerance.ToString();
        }
    }
}
