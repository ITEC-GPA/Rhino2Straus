using System;
using Rhino2Straus.Core.Helper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2Straus.Core.Elements;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_NodeElement : GH_GeometricGoo<NodeElementModel>, IGH_PreviewData
    {
        public Guid Guid { get; set; }

        public override bool IsValid => Value != null;

        public override string TypeName => "Node";

        public override string TypeDescription => "Node";

        BoundingBox IGH_PreviewData.ClippingBox => Boundingbox;

        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;
                else
                    return new BoundingBox(Value.Position, Value.Position);
            }
        }

        public GH_NodeElement(NodeElementModel nodeModel, Guid? guid = null)
        {
            if (nodeModel != null)
                Value = new NodeElementModel(nodeModel);
            else
                Value = new NodeElementModel();

            if (guid != null && guid.HasValue)
                Guid = guid.Value;
        }

        public GH_NodeElement()
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GH_NodeElement(new NodeElementModel(Value));
        }

        public override string ToString()
        {
            return $"X: {Value.X}, Y: {Value.Y}, Z: {Value.Z}";
        }

        void IGH_PreviewData.DrawViewportWires(GH_PreviewWireArgs args)
        {
            Value.DrawWireframe(args.Pipeline, args.Viewport, args.Color);
        }

        void IGH_PreviewData.DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            Value.DrawWireframe(args.Pipeline, args.Viewport, args.Material.Diffuse);
        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return new GH_NodeElement(new NodeElementModel(Value));
        }

        public override BoundingBox GetBoundingBox(Transform xform)
        {
            var box=Boundingbox;if(box.IsValid)box.Transform(xform);return box;
        }

        public override IGH_GeometricGoo Transform(Transform xform)
        {
            return new GH_NodeElement(ElementTransforms.Node(Value,xform));
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            throw new NotSupportedException("FEM elements support rigid placement only. Recreate the elements from morphed geometry and reassign properties.");
        }

        public override bool CastTo<Q>(out Q target)
        {
            if (typeof(Q) == typeof(GH_Point))
            {
                Point3d p = new Point3d(Value.Position);
                target = (Q)(object)new GH_Point(p);
                return true;
            }

            target = default(Q);
            return false;
        }
    }
}
