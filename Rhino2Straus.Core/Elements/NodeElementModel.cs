using Rhino.Display;
using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Base;
using Rhino2Straus.Core.Loads;
using Rhino2Straus.Core.Models;
using System.Collections.Generic;
using System.Drawing;

namespace Rhino2Straus.Core.Elements
{
    public class NodeElementModel : ElementModel
    {
        public List<NodeTemperatureModel> Temperatures { get; set; } = new List<NodeTemperatureModel>();
        public Vector3d TranslationSpring { get; set; }
        public Vector3d RotationSpring { get; set; }
        public Vector3d TranslationMass { get; set; }
        public Vector3d RotationMass { get; set; }

        public double[] PrescribedDisplacement { get; set; } = new double[6];

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }
        public List<NodalLoadModel> NodalLoadList { get; set; }

        public NodeSupportModel Support { get; set; }

        public Point3d Position => new Point3d(X, Y, Z);

        public NodeElementModel(Point3d position, List<ElementGroupModel> group = null, NodeSupportModel support = null, List<NodalLoadModel> nodalLoadList = null)
            : this(position.X, position.Y, position.Z, group, support, nodalLoadList)
        {
        }

        public NodeElementModel(double x, double y, double z, List<ElementGroupModel> group = null, NodeSupportModel support = null, List<NodalLoadModel> nodalLoadList = null)
            : base()
        {
            X = x;
            Y = y;
            Z = z;
            if (nodalLoadList != null)
                NodalLoadList = nodalLoadList;
            else
                NodalLoadList = new List<NodalLoadModel>();

            if (group != null)
                Groups = group;
            else
                Groups = new List<ElementGroupModel>();

            if (support != null)
                Support = support;
            else
                Support = new NodeSupportModel();
        }

        public NodeElementModel(NodeElementModel nodeModel)
            : base(nodeModel)
        {
            Temperatures = nodeModel.Temperatures.ConvertAll(t => new NodeTemperatureModel { LoadCase = t.LoadCase, Temperature = t.Temperature, Type=t.Type });
            PrescribedDisplacement = (double[])nodeModel.PrescribedDisplacement.Clone();
            TranslationSpring = nodeModel.TranslationSpring;
            RotationSpring = nodeModel.RotationSpring;
            TranslationMass = nodeModel.TranslationMass;
            RotationMass = nodeModel.RotationMass;
            X = nodeModel.X;
            Y = nodeModel.Y;
            Z = nodeModel.Z;
            NodalLoadList = nodeModel.NodalLoadList.ConvertAll(x => new NodalLoadModel(x));
            Groups = new List<ElementGroupModel>(nodeModel.Groups);
            Support = new NodeSupportModel(nodeModel.Support);
        }

        public NodeElementModel()
            :base()
        {
            NodalLoadList = new List<NodalLoadModel>();
            Support = new NodeSupportModel();
        }

        public void DrawWireframe(DisplayPipeline display, RhinoViewport viewport, Color color)
        {
            display.DrawPoint(Position, color);
        }

        public void DrawSolid(DisplayPipeline display, RhinoViewport viewport, Color color)
        {

        }

        public static int Comparer(NodeElementModel a, NodeElementModel b)
        {
            var rx = a.Position.X.CompareTo(b.Position.X);
            if (rx == 0)
            {
                var ry = a.Position.Y.CompareTo(b.Position.Y);

                if (ry == 0)
                    return a.Position.Z.CompareTo(b.Position.Z);
                else
                    return ry;
            }
            return rx;
        }
    }
}
