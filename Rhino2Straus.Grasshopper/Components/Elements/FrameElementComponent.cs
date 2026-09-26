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
    public class FrameElementComponent : SafeComponent
    {
        public FrameElementComponent()
            : base("Frame element", "Frame element", "Frame element", ComponentTopics.Tab, ComponentTopics.Elements)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Frame line", "Frame line", "Frame line", GH_ParamAccess.item);
            pManager.AddGenericParameter("Frame Property", "Frame Property", "Frame Property", GH_ParamAccess.item);
            pManager.AddAngleParameter("Angle (deg)", "Angle (deg)", "Angle (deg)", GH_ParamAccess.item, 0.0);
            pManager.AddGenericParameter("Groups", "Groups", "Groups", GH_ParamAccess.list);
            ((GH_ParamManager)pManager)[3].Optional = true;
            pManager.AddVectorParameter("Offset", "Offset", "Offset", GH_ParamAccess.item, Vector3d.Zero);
            ((GH_ParamManager)pManager)[4].Optional = true;
            pManager.AddIntegerParameter("Frame ID", "Frame ID", "Frame ID", GH_ParamAccess.item, ModelObjectId.UNASSIGNED);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame element", "Frame element", "Frame element", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            Curve val = null;
            GH_FrameProperty gH_FrameProperty = null;
            List<GH_ElementGroup> groups = new List<GH_ElementGroup>();
            double angle = 0;
            int id = 0;
            Vector3d offset = Vector3d.Zero;

            if (!DA.GetData(0, ref val) || !DA.GetData(1, ref gH_FrameProperty))
                return;
            DA.GetData(2, ref angle);
            DA.GetDataList(3, groups);
            DA.GetData(4, ref offset);
            DA.GetData(5, ref id);

            if (!val.IsLinear()) throw new ArgumentException("Frame geometry must be a straight line. Use Polyline to Beams for multiple segments.");
            NodeElementModel startNode = new NodeElementModel(val.PointAtStart.X, val.PointAtStart.Y, val.PointAtStart.Z, null, null, null);
            NodeElementModel endNode = new NodeElementModel(val.PointAtEnd.X, val.PointAtEnd.Y, val.PointAtEnd.Z, null, null, null);
            FrameElementModel frameElement = new FrameElementModel(startNode, endNode, gH_FrameProperty.Value, Rhino.RhinoMath.ToRadians(angle), offset)
            { Id = id };

            if (groups != null && groups.Count > 0)
            {
                List<ElementGroupModel> elementGroupModels = new List<ElementGroupModel>();
                for (int i = 0; i < groups.Count; i++)
                    elementGroupModels.Add(groups[i].Value);

                frameElement.Groups = elementGroupModels;
            }
            DA.SetData(0, new GH_FrameElement(frameElement));
        }


        public override Guid ComponentGuid => new Guid("07d14640-7b14-4515-ad1d-99f508fa19d3");
    }
}
