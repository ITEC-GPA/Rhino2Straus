using Rhino2Straus.Grasshopper;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Base;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Grasshopper.Datatype;

namespace Rhino2Straus.Grasshopper.Straus.Components.Elements
{
    public class LinkElementComponent : SafeComponent
    {
        public LinkElementComponent()
            : base("Link element", "Link element", "Link element", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ELEMENTS)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Start Point", "Start Point", "Start Point", GH_ParamAccess.item);
            pManager.AddPointParameter("End Point", "End Point", "End Point", GH_ParamAccess.item);
            pManager.AddGenericParameter("Link Property", "Link Property", "Link Property", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Link", "Link", "Link", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            Point3d pt1 = Point3d.Unset;
            Point3d pt2 = Point3d.Unset;
            GH_LinkProperty gH_FrameProperty = null;

            if (!DA.GetData(0, ref pt1) || !DA.GetData(1, ref pt2) || !DA.GetData(2, ref gH_FrameProperty))
                return;

            NodeElementModel startNode = new NodeElementModel(pt1);
            NodeElementModel endNode = new NodeElementModel(pt2);
            LinkElementModel link = new LinkElementModel(startNode, endNode, gH_FrameProperty.Value);
            DA.SetData(0, new GH_LinkElement(link));
        }


        public override Guid ComponentGuid => new Guid("56b5da2b-0d9f-488c-96bf-2a6e50027414");
    }
}
