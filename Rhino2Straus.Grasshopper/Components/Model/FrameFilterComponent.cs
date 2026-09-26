using Rhino2Straus.Grasshopper;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2Straus.Grasshopper.Datatype;

namespace Rhino2Straus.Grasshopper.Straus.Components.Models
{
    public class FrameFilterComponent : SafeComponent
    {
        public FrameFilterComponent()
            : base("Frame Filter", "Frame Filter", "Frame Filter", ComponentTopics.Tab, ComponentTopics.Model)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Node elements", "Node elements", "Node elements", GH_ParamAccess.list);
            pManager.AddPointParameter("Start Position", "Start Position", "Start Position", GH_ParamAccess.item);
            pManager.AddPointParameter("End Position", "End Position", "End Position", GH_ParamAccess.item);
            pManager.AddGenericParameter("Frame Properties", "Frame Properties", "Frame Properties", GH_ParamAccess.list);
            pManager.AddGenericParameter("Groups", "Groups", "Groups", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance", GH_ParamAccess.item, 0.001);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Filtered Frames", "Filtered Frames", "Filtered Frames", GH_ParamAccess.list);
            pManager.AddGenericParameter("Other Frames", "Other Frames", "Other Frames", GH_ParamAccess.list);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            List<GH_FrameElement> gh_Frames = new List<GH_FrameElement>();
            Point3d startNode = Point3d.Unset;
            Point3d endNode = Point3d.Unset;
            List<GH_ElementGroup> gH_ElementGroups = new List<GH_ElementGroup>();
            List<GH_FrameSection> gH_FrameProperties = new List<GH_FrameSection>();
            double tolerance = 0;

            int count = 0;
            if (!DA.GetDataList(count++, gh_Frames))
                return;
            DA.GetData(count++, ref startNode);
            DA.GetData(count++, ref endNode);
            DA.GetDataList(count++, gH_FrameProperties);
            DA.GetDataList(count++, gH_ElementGroups);
            DA.GetData(count++, ref tolerance);

            List<GH_FrameElement> matchedFrames = new List<GH_FrameElement>();
            List<GH_FrameElement> otherFrames = new List<GH_FrameElement>();


            List<string> groupName = new List<string>();
            for (int i = 0; i < gH_ElementGroups.Count; i++)
                groupName.Add(gH_ElementGroups[i].Value.Name);
            List<string> propName = new List<string>();
            for (int i = 0; i < gH_FrameProperties.Count; i++)
                propName.Add(gH_FrameProperties[i].Value.Name);

            for (int i = 0; i < gh_Frames.Count; i++)
            {
                bool find = false;

                Core.Elements.FrameElementModel node = gh_Frames[i].Value;
                if (!find && groupName.Contains(node.Name))
                    find = true;

                if (!find)
                {
                    if (gh_Frames[i].Value.NodeStart.Position.DistanceTo(startNode) < tolerance)
                        find = true;
                    if (gh_Frames[i].Value.NodeEnd.Position.DistanceTo(endNode) < tolerance)
                        find = true;
                }

                if (!find)
                {
                    if (propName.Contains(gh_Frames[i].Value.FrameSection.Name))
                        find = true;
                }

                if (find)
                    matchedFrames.Add(gh_Frames[i]);
                else
                    otherFrames.Add(gh_Frames[i]);
            }

            DA.SetDataList(0, matchedFrames);
            DA.SetDataList(1, otherFrames);
        }


        public override Guid ComponentGuid => new Guid("38a772ad-451e-4af6-bead-52f530028b8b");
    }
}
