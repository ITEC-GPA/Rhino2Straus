using Rhino2Straus.Grasshopper;
using System;
using Grasshopper.Kernel;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Grasshopper.Datatype;

namespace Rhino2Straus.Grasshopper.Straus.Components.Attributes
{
    public class NodeSupportComponent : SafeComponent
    {
        public NodeSupportComponent()
            : base("Support", "Support", "Support", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ATTRIBUTES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Dx", "Dx", "Dx", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Dy", "Dy", "Dy", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Dz", "Dz", "Dz", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Mx", "Mx", "Mx", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("My", "My", "My", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Mz", "Mz", "Mz", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            GH_NodeElement node = null;
            bool dx = false;
            bool dy = false;
            bool dz = false;
            bool mx = false;
            bool my = false;
            bool mz = false;

            if (!DA.GetData(0, ref node))
                return;

            if (DA.GetData(1, ref dx) && DA.GetData(2, ref dy) && DA.GetData(3, ref dz) && DA.GetData(4, ref mx) && DA.GetData(5, ref my) && DA.GetData(6, ref mz))
            {
                NodeSupportModel support = new NodeSupportModel(dx, dy, dz, mx, my, mz);
                NodeElementModel newNode = new NodeElementModel(node.Value) { Support = support };
                DA.SetData(0, new GH_NodeElement(newNode));
            }
        }


        public override Guid ComponentGuid => new Guid("3ed0cf37-cc66-4e44-97ed-e6c77b11b662");
    }
}
