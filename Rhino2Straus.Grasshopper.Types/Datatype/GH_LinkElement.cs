using System;
using Rhino2Straus.Core.Helper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2Straus.Core.Elements;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_LinkElement : GH_GeometricGoo<LinkElementModel>, IGH_PreviewData
    {
        public Guid Guid { get; set; }

        public override bool IsValid => Value != null;

        public override string TypeName => "Link";

        public override string TypeDescription => "Link";

        BoundingBox IGH_PreviewData.ClippingBox => PreviewPolicy.AutomaticVisible()?Boundingbox:BoundingBox.Empty;

        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;
                else
                    return new BoundingBox(Value.NodeStart.Position, Value.NodeEnd.Position);
            }
        }

        public GH_LinkElement(LinkElementModel model, Guid? guid = null)
        {
            if (model != null)
                Value = new LinkElementModel(model);
            else
                Value = new LinkElementModel();

            if (guid != null && guid.HasValue)
                Guid = guid.Value;
        }

        public GH_LinkElement()
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GH_LinkElement(new LinkElementModel(Value));
        }

        public override string ToString()
        {
            return $"Link element type {Value.LinkProperty.Type} from point \r" +
                $"X: {Value.NodeStart.X}, Y: {Value.NodeStart.Y}, Z: {Value.NodeStart.Z} to point" +
                $"X: {Value.NodeEnd.X}, Y: {Value.NodeEnd.Y}, Z: {Value.NodeEnd.Z}";
        }

        void IGH_PreviewData.DrawViewportWires(GH_PreviewWireArgs args)
        {
            if(!PreviewPolicy.AutomaticVisible())return;
            Value.DrawWireframe(args.Pipeline, args.Viewport, args.Color);
        }

        void IGH_PreviewData.DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            if(!PreviewPolicy.AutomaticVisible())return;

        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return new GH_LinkElement(new LinkElementModel(Value));
        }

        public override BoundingBox GetBoundingBox(Transform xform)
        {
            var box=Boundingbox;if(box.IsValid)box.Transform(xform);return box;
        }

        public override IGH_GeometricGoo Transform(Transform xform)
        {
            return new GH_LinkElement(ElementTransforms.Link(Value,xform));
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            throw new NotSupportedException("FEM elements support rigid placement only. Recreate the elements from morphed geometry and reassign properties.");
        }
    }
}
