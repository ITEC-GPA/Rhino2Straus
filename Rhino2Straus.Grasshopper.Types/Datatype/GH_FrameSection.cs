using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.ElementProperties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_FrameSection : GH_Goo<FrameSectionModel>, IGH_PreviewData
    {
        public BoundingBox ClippingBox
        {
            get {var box=BoundingBox.Empty;if(PreviewPolicy.AutomaticVisible()&&Value?.Breps!=null)foreach(var b in Value.Breps)box.Union(b.GetBoundingBox(true));return box;}
        }
        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if(PreviewPolicy.AutomaticVisible()&&Value?.Breps!=null)foreach(var b in Value.Breps)args.Pipeline.DrawBrepWires(b,args.Color);
        }
        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            if(PreviewPolicy.AutomaticVisible()&&Value?.Breps!=null)foreach(var b in Value.Breps)args.Pipeline.DrawBrepShaded(b,args.Material);
        }
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
