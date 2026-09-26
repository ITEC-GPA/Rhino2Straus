using Rhino.Display;
using Rhino2Straus.Core.Base;
using Rhino2Straus.Core.ElementProperties;
using System.Drawing;

namespace Rhino2Straus.Core.Elements
{
    public class LinkElementModel : ElementModel
    {
        public NodeElementModel NodeStart { get; set; }

        public NodeElementModel NodeEnd { get; set; }

        public LinkPropertyModel LinkProperty { get; set; }

        public LinkElementModel(NodeElementModel nodeStart, NodeElementModel nodeEnd, LinkPropertyModel linkProperty)
        {
            NodeStart = nodeStart;
            NodeEnd = nodeEnd;
            LinkProperty = linkProperty;
        }

        public LinkElementModel()
            : base()
        {
        }

        public LinkElementModel(LinkElementModel linkElementModel)
            : base(linkElementModel)
        {
            NodeStart = linkElementModel.NodeStart == null ? null : new NodeElementModel(linkElementModel.NodeStart);
            NodeEnd = linkElementModel.NodeEnd == null ? null : new NodeElementModel(linkElementModel.NodeEnd);
            LinkProperty = linkElementModel.LinkProperty == null ? null : new LinkPropertyModel(linkElementModel.LinkProperty);
        }

        public void DrawWireframe(DisplayPipeline display, RhinoViewport viewport, Color color)
        {
            display.DrawLine(new Rhino.Geometry.Line(NodeStart.Position, NodeEnd.Position), color);
        }

        public void DrawSolid(DisplayPipeline display, RhinoViewport viewport, Color color)
        {

        }
    }
}
