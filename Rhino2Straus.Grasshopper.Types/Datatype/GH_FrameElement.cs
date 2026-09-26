using System;
using Rhino2Straus.Core.Helper;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2Straus.Core.Elements;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_FrameElement : GH_GeometricGoo<FrameElementModel>, IGH_PreviewData
    {
        public Guid Guid { get; set; }

        public override bool IsValid => Value != null;

        public override string TypeName => "Frame";

        public override string TypeDescription => "Frame";

        BoundingBox IGH_PreviewData.ClippingBox => PreviewPolicy.AutomaticVisible()?Boundingbox:BoundingBox.Empty;

        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;
                else
                    return PreviewGeometry.BeamBounds(Value);
            }
        }

        public GH_FrameElement(FrameElementModel nodeModel, Guid? guid = null)
        {
            if (nodeModel != null)
                Value = new FrameElementModel(nodeModel);
            else
                Value = new FrameElementModel();

            if (guid != null && guid.HasValue)
                Guid = guid.Value;
        }

        public GH_FrameElement()
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GH_FrameElement(new FrameElementModel(Value));
        }

        public override string ToString()
        {
            return $"Frame element from point \r" +
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
            Value.DrawSolid(args.Pipeline, args.Viewport, args.Material.Diffuse);
        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return new GH_FrameElement(new FrameElementModel(Value));
        }

        public override BoundingBox GetBoundingBox(Transform xform)
        {
            var box=Boundingbox;if(box.IsValid)box.Transform(xform);return box;
        }

        public override IGH_GeometricGoo Transform(Transform xform)
        {
            return new GH_FrameElement(ElementTransforms.Beam(Value,xform));
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            throw new NotSupportedException("FEM elements support rigid placement only. Recreate the elements from morphed geometry and reassign properties.");
        }

        #region Casting methods

        public override bool CastTo<Q>(out Q target)
        {
            if (typeof(Q) == typeof(GH_Curve))
            {
                LineCurve pline = new LineCurve(Value.NodeStart.Position, Value.NodeEnd.Position);
                target = (Q)(object)new GH_Curve(pline);
                return true;
            }
            else if (typeof(Q) == typeof(GH_Brep))
            {
                if (Value.Breps == null || Value.Breps.Count == 0) Value.BuildBreps();
                if (Value.Breps == null || Value.Breps.Count == 0)
                {
                    target = default(Q);
                    return false;
                }
                if (Value.Breps.Count == 1)
                {
                    Brep brep = Value.Breps[0].DuplicateBrep();
                    target = (Q)(object)new GH_Brep(brep);
                    return true;
                }
            }

            target = default(Q);
            return false;
        }

        #endregion
    }
}
