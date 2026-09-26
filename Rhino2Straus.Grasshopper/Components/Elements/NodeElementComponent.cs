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
    public class NodeElementComponent : SafeComponent
    {
        public NodeElementComponent()
            : base("Node Element", "Node Element", "Node Element", ComponentTopics.Tab, ComponentTopics.Elements)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Node point", "Node point", "Node point", GH_ParamAccess.item);
            pManager.AddGenericParameter("Group", "Group", "Group", GH_ParamAccess.list);
            ((GH_ParamManager)pManager)[1].Optional = true;
            pManager.AddIntegerParameter("Node ID", "Node ID", "Node ID", GH_ParamAccess.item, ModelObjectId.UNASSIGNED);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            Point3d val = Point3d.Unset;
            List<GH_ElementGroup> groups = new List<GH_ElementGroup>();
            int id = 0;

            if (!DA.GetData(0, ref val))
                return;
            DA.GetDataList(1, groups);
            DA.GetData(2, ref id);

            NodeElementModel node = new NodeElementModel(val) { Id = id };

            if (groups != null && groups.Count > 0)
            {
                List<ElementGroupModel> elementGroupModels = new List<ElementGroupModel>();
                for (int i = 0; i < groups.Count; i++)
                    elementGroupModels.Add(groups[i].Value);

                node.Groups = elementGroupModels;
            }
            DA.SetData(0, new GH_NodeElement(node));
        }


        public override Guid ComponentGuid => new Guid("1ea612a7-69e3-4993-8970-1efcb142fbe9");
    }
}
