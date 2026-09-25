using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.ElementProperties;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_FrameSection : GH_Goo<FrameSectionModel>
    {
        public GH_FrameSection(FrameSectionModel model)
        {
            Value = model;
        }

        public GH_FrameSection()
        {

        }

        public override bool IsValid => Value != null;

        public override string TypeName => "FrameSectionModel";

        public override string TypeDescription => "FrameSectionModel";

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new GH_FrameSection() : new GH_FrameSection(new FrameSectionModel(Value));
        }

        public override string ToString()
        {
            return "Name: " + Value.Name + ", \r" +
                "Type: " + Value.SectionGeometryType;
        }
    }
}
