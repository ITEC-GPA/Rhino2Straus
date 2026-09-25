using Rhino.Display;
using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Base;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Loads;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Rhino2Straus.Core.Elements
{
    public class AreaElementModel : ElementModel
    {
        public enum PlateType
        {
            Invalid,
            Tri3,
            Quad4,
            Generic,
        }

        public List<NodeElementModel> NodeList { get; set; }

        public AreaThicknessModel AreaThickness { get; set; }

        public AreaPropertyModel AreaProperty { get; set; }

        public MaterialModel Material { get; set; }

        public List<AreaLoadBaseModel> AreaLoadList { get; set; }

        public double Angle { get; set; }

        public double Offset { get; set; }

        public Brep Brep { get; set; }

        public PlateType Type
        {
            get
            {
                switch (NodeList.Count)
                {
                    case 3:
                        return PlateType.Tri3;
                    case 4:
                        return PlateType.Quad4;
                }
                return PlateType.Generic;
            }
        }

        public AreaElementModel(List<NodeElementModel> nodeList, AreaPropertyModel areaPropertyModel, double angle = 0, double offset = 0, List<ElementGroupModel> group = null, List<AreaLoadBaseModel> areaLoadList = null)
        : base()
        {
            NodeList = nodeList;
            AreaProperty = areaPropertyModel;
            AreaThickness = areaPropertyModel.Section;
            Material = areaPropertyModel.Material;
            Angle = angle;
            Offset = offset;
            if (areaLoadList != null)
                AreaLoadList = areaLoadList;
            else
                AreaLoadList = new List<AreaLoadBaseModel>();
            if (group != null)
                Groups = group;
            else
                Groups = new List<ElementGroupModel>();

        }

        public AreaElementModel(List<NodeElementModel> nodeList, AreaThicknessModel areaThickness, MaterialModel material, double angle = 0, double offset = 0, List<ElementGroupModel> group = null, List<AreaLoadBaseModel> areaLoadList = null)
            : this(nodeList, new AreaPropertyModel(areaThickness.Name, material, areaThickness), angle, offset, group, areaLoadList)
        {

        }

        public AreaElementModel()
            : base()
        {
        }


        public AreaElementModel(AreaElementModel areaElementModel)
            : base(areaElementModel)
        {
            NodeList = new List<NodeElementModel>(areaElementModel.NodeList);
            AreaProperty = areaElementModel.AreaProperty;
            AreaThickness = areaElementModel.AreaThickness;
            Material = areaElementModel.Material;
            Angle = areaElementModel.Angle;
            Groups = new List<ElementGroupModel>(areaElementModel.Groups);
            AreaLoadList = areaElementModel.AreaLoadList.ConvertAll(x=>x.Duplicate());
            Brep = null;
            Offset = areaElementModel.Offset;
        }

        public void DrawWireframe(DisplayPipeline display, RhinoViewport viewport, Color color)
        {
            if (Brep == null)
                BuildBrep();
            if (Brep != null)
                display.DrawBrepWires(Brep, color);
        }

        public void DrawSolid(DisplayPipeline display, RhinoViewport viewport, Color color)
        {
            if (Brep == null)
                BuildBrep();
            if (Brep != null)
                display.DrawBrepShaded(Brep, new DisplayMaterial(color));
        }

        public void BuildBrep()
        {
            var mesh = Rhino2Straus.Core.Helper.PreviewGeometry.Plate(this);
            Brep = mesh.Faces.Count == 0 ? null : Rhino.Geometry.Brep.CreateFromMesh(mesh, true);
        }

        public void GetLocalAxes(out Vector3d axes1, out Vector3d axes2, out Vector3d axes3)
        {
            //orientation like Straus7
            if (Type == PlateType.Tri3)
            {
                //   3
                //
                //1     2
                //xAxes from 1 to mid23
                Vector3d from2To3 = NodeList[2].Position - NodeList[1].Position;
                Point3d mid23 = NodeList[1].Position + 0.5 * from2To3;
                axes1 = mid23 - NodeList[0].Position;
                axes1.Unitize();
                Vector3d dir0 = NodeList[1].Position - NodeList[0].Position;
                Vector3d dir1 = NodeList[2].Position - NodeList[1].Position;
                axes3 = Vector3d.CrossProduct(dir0, dir1);
                axes3.Unitize();
                axes2 = Vector3d.CrossProduct(axes3, axes1);
                axes2.Unitize();
            }
            else if (Type == PlateType.Quad4)
            {
                //4      3
                //
                //
                //1      2
                //xAxes from mid 14 to mid23
                Vector3d from1To4 = NodeList[3].Position - NodeList[0].Position;
                Vector3d from2To3 = NodeList[2].Position - NodeList[1].Position;
                Point3d mid14 = NodeList[0].Position + 0.5 * from1To4;
                Point3d mid23 = NodeList[1].Position + 0.5 * from2To3;
                axes1 = mid23 - mid14;
                axes1.Unitize();
                Vector3d dir0 = NodeList[1].Position - NodeList[0].Position;
                dir0.Unitize();
                Vector3d dir1 = NodeList[2].Position - NodeList[1].Position;
                dir1.Unitize();
                if (Vector3d.Multiply(dir0, dir1) > 0.99999)
                {
                    from1To4.Unitize();
                    dir1 = from1To4;
                }
                axes3 = Vector3d.CrossProduct(dir0, dir1);
                axes2 = Vector3d.CrossProduct(axes3, axes1);
                axes2.Unitize();
                //need a recalculation of zdir, points could not be on a plane
                axes3 = Vector3d.CrossProduct(axes1, axes2);
                axes3.Unitize();
            }
            else
            {
                //4      3
                //
                //
                //1      2
                //xAxes from mid 14 to mid23
                Vector3d from1To4, from2To3, dir0, dir1;
                Point3d mid14, mid23;
                from1To4 = NodeList[3].Position - NodeList[0].Position;
                from2To3 = NodeList[2].Position - NodeList[1].Position;
                mid14 = NodeList[0].Position + 0.5 * from1To4;
                mid23 = NodeList[1].Position + 0.5 * from2To3;
                axes1 = mid23 - mid14;
                axes1.Unitize();
                dir0 = NodeList[1].Position - NodeList[0].Position;
                dir0.Unitize();
                dir1 = NodeList[2].Position - NodeList[1].Position;
                dir1.Unitize();
                if (Vector3d.Multiply(dir0, dir1) > 0.99999)
                {
                    from1To4.Unitize();
                    dir1 = from1To4;
                }
                axes3 = Vector3d.CrossProduct(dir0, dir1);
                axes2 = Vector3d.CrossProduct(axes3, axes1);
                axes2.Unitize();
                //need a recalculation of zdir, points could not be on a plane
                axes3 = Vector3d.CrossProduct(axes1, axes2);
                axes3.Unitize();
            }

            //Rotating of angle
            if (Angle != 0)
            {
                double radAngle = Angle;
                Point3d pOnX = new Point3d(axes1.X, axes1.Y, axes1.Z);
                Point3d pOnY = new Point3d(axes2.X, axes2.Y, axes2.Z);
                Plane plane = new Plane(Point3d.Origin, pOnX, pOnY);

                Transform trans = Transform.PlaneToPlane(Plane.WorldXY, plane);
                Point3d newDir = new Point3d(Math.Cos(radAngle), Math.Sin(radAngle), 0);
                newDir.Transform(trans);

                axes1 = new Vector3d(newDir.X, newDir.Y, newDir.Z);
                axes2 = Vector3d.CrossProduct(axes3, axes1);
            }
        }
    }
}
