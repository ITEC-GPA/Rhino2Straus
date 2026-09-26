using System;
using Rhino2Straus.Core.Helper;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2Straus.Core.Elements;

namespace Rhino2Straus.Grasshopper.Datatype
{
    public class GH_AreaElement : GH_GeometricGoo<AreaElementModel>, IGH_PreviewData
    {
        public Guid Guid { get; set; }

        public override bool IsValid => Value != null;

        public override string TypeName => "Area";

        public override string TypeDescription => "Area";

        BoundingBox IGH_PreviewData.ClippingBox => PreviewPolicy.AutomaticVisible()?Boundingbox:BoundingBox.Empty;

        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;
                else
                {
                    var bb = PreviewGeometry.Plate(Value).GetBoundingBox(true);
                    return bb;
                }
            }
        }

        public GH_AreaElement(AreaElementModel nodeModel, Guid? guid = null)
        {
            if (nodeModel != null)
                Value = new AreaElementModel(nodeModel);
            else
                Value = new AreaElementModel();

            if (guid != null && guid.HasValue)
                Guid = guid.Value;
        }

        public GH_AreaElement()
        {
        }

        public override IGH_Goo Duplicate()
        {
            return new GH_AreaElement(new AreaElementModel(Value));
        }

        public override string ToString()
        {
            return $"Area element";
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
            return new GH_AreaElement(new AreaElementModel(Value));
        }

        public override BoundingBox GetBoundingBox(Transform xform)
        {
            var box=Boundingbox;if(box.IsValid)box.Transform(xform);return box;
        }

        public override IGH_GeometricGoo Transform(Transform xform)
        {
            return new GH_AreaElement(ElementTransforms.Plate(Value,xform));
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
                PolylineCurve pline = new PolylineCurve(Value.NodeList.Select(i => i.Position));
                pline.MakeClosed(0);
                target = (Q)(object)new GH_Curve(pline);
                return true;
            }
            else if (typeof(Q) == typeof(GH_Brep))
            {
                if(Value.Brep==null)Value.BuildBrep();
                target = (Q)(object)new GH_Brep(Value.Brep);
                return true;
            }

            target = default(Q);
            return false;
        }

        #endregion
    }
}
