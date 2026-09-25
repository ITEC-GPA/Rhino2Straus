using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.ElementProperties;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_FrameProperty : GH_Goo<FramePropertyModel>
    {
        public GH_FrameProperty(FramePropertyModel model)
        {
            Value = model;
        }

        public GH_FrameProperty()
        {

        }

        public override bool IsValid => Value != null;

        public override string TypeName => "FrameProperty";

        public override string TypeDescription => "FrameProperty";

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new GH_FrameProperty() : new GH_FrameProperty(new FramePropertyModel(Value));
        }

        public override string ToString()
        {
            return "Name: " + Value.Name + ", \r" +
                "Type: " + Value.SectionGeometryType;
        }
    }
}
