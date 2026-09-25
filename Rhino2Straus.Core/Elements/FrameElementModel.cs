using Rhino.Display;
using System;
using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Base;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Loads;
using System.Collections.Generic;
using System.Drawing;

namespace Rhino2Straus.Core.Elements
{
    public class FrameElementModel : ElementModel
    {
        public bool[] StartReleases { get; set; } = new bool[6];
        public bool[] EndReleases { get; set; } = new bool[6];

        public NodeElementModel NodeStart { get; set; }

        public NodeElementModel NodeEnd { get; set; }

        public FrameSectionModel FrameSection { get; set; }

        public FramePropertyModel FrameProperty { get; set; }

        public MaterialModel Material { get; set; }

        public List<FrameLoadBaseModel> FrameLoadList { get; set; }

        public Vector3d Offset { get; set; }

        public double OffsetX
        {
            get => Offset.X;
            set => Offset = new Vector3d(value, Offset.Y, Offset.Z);
        }

        public double OffsetY
        {
            get => Offset.Y;
            set => Offset = new Vector3d(Offset.X, value, Offset.Z);
        }

        public double Angle { get; set; }

        public List<Brep> Breps { get; set; }

        public bool IsVertical
        {
            get
            {
                Vector3d v = NodeEnd.Position - NodeStart.Position;
                v.Unitize();
                return Vector3d.CrossProduct(Vector3d.ZAxis, v).Length < 1.0E-12;
            }
        }

        public FrameElementModel(NodeElementModel startNode, NodeElementModel endNode, FramePropertyModel frameProperty, double angle, Vector3d offset, List<ElementGroupModel> group = null, List<FrameLoadBaseModel> frameLoadList = null)
            : base()
        {
            NodeStart = startNode;
            NodeEnd = endNode;
            FrameProperty = frameProperty;
            FrameSection = frameProperty.Section;
            Material = frameProperty.Material;
            Offset = offset;
            if (frameLoadList != null)
                FrameLoadList = frameLoadList;
            else
                FrameLoadList = new List<FrameLoadBaseModel>();
            Angle = angle;
            if (group != null)
                Groups = group;
            else
                Groups = new List<ElementGroupModel>();
            Breps = new List<Brep>();

        }

        public FrameElementModel(NodeElementModel startNode, NodeElementModel endNode, FrameSectionModel frameProperty, MaterialModel material, double angle, Vector3d offset, List<ElementGroupModel> group = null, List<FrameLoadBaseModel> frameLoadList = null)
            : this(startNode, endNode, new FramePropertyModel(frameProperty.Name, material, frameProperty), angle, offset, group, frameLoadList)
        {
        }

        public FrameElementModel()
            : base()
        {
            FrameLoadList = new List<FrameLoadBaseModel>();
            Groups = new List<ElementGroupModel>();
            Breps = new List<Brep>();
        }

        public FrameElementModel(FrameElementModel frameElementModel)
            : base(frameElementModel)
        {
            NodeStart = frameElementModel.NodeStart;
            NodeEnd = frameElementModel.NodeEnd;
            FrameProperty = frameElementModel.FrameProperty;
            FrameSection = frameElementModel.FrameSection;
            Material = frameElementModel.Material;
            StartReleases = (bool[])frameElementModel.StartReleases.Clone();
            EndReleases = (bool[])frameElementModel.EndReleases.Clone();
            Angle = frameElementModel.Angle;
            Groups = new List<ElementGroupModel>(frameElementModel.Groups);
            FrameLoadList = frameElementModel.FrameLoadList.ConvertAll(x=>x.Duplicate());
            Offset = frameElementModel.Offset;
            Breps = new List<Brep>();
        }

        public void DrawWireframe(DisplayPipeline display, RhinoViewport viewport, Color color)
        {
            display.DrawLine(new Line(NodeStart.Position, NodeEnd.Position), color);
        }

        public void DrawSolid(DisplayPipeline display, RhinoViewport viewport, Color color)
        {
            if (Breps == null || Breps.Count == 0) BuildBreps();
            if (Breps != null)
            {
                foreach (Brep shape in Breps)
                    display.DrawBrepShaded(shape, new DisplayMaterial(color));
            }
        }

        public void BuildBreps() => Breps = Rhino2Straus.Core.Helper.PreviewGeometry.Beam(this);

        public void GetLocalAxes(bool getPrincipal, out Vector3d axes3, out Vector3d axes2, out Vector3d axes1, out Transform toGlobal)
        {
            double r = Angle;
            if (!getPrincipal)
            {
                r -= FrameSection.Angle;
            }
            axes3 = NodeEnd.Position - NodeStart.Position;
            axes3.Unitize();
            if (!IsVertical) // Not vertical
            {
                Vector3d v_xy = new Vector3d(axes3.X, axes3.Y, 0);
                v_xy.Unitize();

                axes1 = Vector3d.CrossProduct(Vector3d.ZAxis, axes3);
                axes1.Unitize();
                axes2 = Vector3d.CrossProduct(axes3, axes1);
                axes2.Unitize();
            }
            else // Vertical
            {
                axes2 = Vector3d.CrossProduct(axes3, Vector3d.YAxis);
                axes2.Unitize();
                axes1 = Vector3d.CrossProduct(axes2, axes3);
                axes1.Unitize();
            }
            Plane pl = new Plane(NodeStart.Position, axes1, axes2);
            pl.Rotate(r, pl.Normal);
            axes1 = pl.XAxis;
            axes2 = pl.YAxis;
            toGlobal = Transform.PlaneToPlane(Plane.WorldXY, pl);
        }

        public Vector3d GetSectionOffset()
        {
            if (FrameSection?.Breps == null || FrameSection.Breps.Count == 0) return Vector3d.Zero;
            var box = BoundingBox.Empty;
            foreach (var brep in FrameSection.Breps) box.Union(brep.GetBoundingBox(true));
            string code = FrameSection.Offset.ToString();
            double x = code[0] == 'L' ? box.Min.X : code[0] == 'R' ? box.Max.X : 0;
            double y = code[1] == 'T' ? box.Max.Y : code[1] == 'B' ? box.Min.Y : 0;
            return new Vector3d(-x, -y, 0);
        }

        public Vector3d GetEffectiveOffset()
        {
            var local = GetSectionOffset();
            double angle = FrameSection?.Angle ?? 0;
            var extra = Offset == Vector3d.Unset ? Vector3d.Zero : Offset;
            return new Vector3d(local.X * Math.Cos(angle) + local.Y * Math.Sin(angle) + extra.X,
                -local.X * Math.Sin(angle) + local.Y * Math.Cos(angle) + extra.Y, 0);
        }
    }
}
