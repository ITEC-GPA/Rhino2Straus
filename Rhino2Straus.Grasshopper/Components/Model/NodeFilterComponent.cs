using Rhino2Straus.Grasshopper;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2Straus.Grasshopper.Datatype;

namespace Rhino2Straus.Grasshopper.Straus.Components.Models
{
    public class NodeFilterComponent : SafeComponent
    {
        public NodeFilterComponent()
            : base("Node Filter", "Node Filter", "Node Filter", ComponentTopics.Tab, ComponentTopics.Model)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Node elements", "Node elements", "Nodes to filter. With no criteria, all nodes pass. Supplied criteria are combined with OR.", GH_ParamAccess.list);
            pManager.AddPointParameter("Position", "Position", "Position", GH_ParamAccess.list);
            pManager.AddGenericParameter("Groups", "Groups", "Groups", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance", GH_ParamAccess.item, 0.001);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Filtered Nodes", "Filtered Nodes", "Filtered Nodes", GH_ParamAccess.list);
            pManager.AddGenericParameter("Other Nodes", "Other Nodes", "Other Nodes", GH_ParamAccess.list);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            List<GH_NodeElement> gh_Nodes = new List<GH_NodeElement>();
            List<Point3d> point3D = new List<Point3d>();
            List<GH_ElementGroup> gH_ElementGroups = new List<GH_ElementGroup>();
            double tolerance = 0;

            int count = 0;
            if (!DA.GetDataList(count++, gh_Nodes))
                return;
            DA.GetDataList(count++, point3D);
            DA.GetDataList(count++, gH_ElementGroups);
            DA.GetData(count++, ref tolerance);

            List<GH_NodeElement> matchedNodes = new List<GH_NodeElement>();
            List<GH_NodeElement> otherNodes = new List<GH_NodeElement>();


            List<string> groupName = new List<string>();
            for (int i = 0; i < gH_ElementGroups.Count; i++)
                groupName.Add(gH_ElementGroups[i].Value.Name);

            for (int i = 0; i < gh_Nodes.Count; i++)
            {
                bool find = groupName.Count==0&&point3D.Count==0;

                Core.Elements.NodeElementModel node = gh_Nodes[i].Value;
                if (!find && node.Groups.Exists(g=>groupName.Contains(g.Name)))
                    find = true;

                if (!find)
                {
                    for(int k = 0; k < point3D.Count; k++)
                        if (gh_Nodes[i].Value.Position.DistanceTo(point3D[k]) < tolerance)
                            find = true;
                }

                if (find)
                    matchedNodes.Add(gh_Nodes[i]);
                else
                    otherNodes.Add(gh_Nodes[i]);
            }

            DA.SetDataList(0, matchedNodes);
            DA.SetDataList(1, otherNodes);
        }


        public override Guid ComponentGuid => new Guid("be7ac363-0cc9-4bed-8720-06af29ef7ef3");
    }
}
