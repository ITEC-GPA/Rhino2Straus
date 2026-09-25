using Rhino2Straus.Grasshopper;
using System;
using Grasshopper.Kernel;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Grasshopper.Datatype;

namespace Rhino2Straus.Grasshopper.Straus.Components.Attributes
{
    public class ElementGroupComponent : SafeComponent
    {
        public ElementGroupComponent()
            : base("Element Group", "Element Group", "Element Group", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ATTRIBUTES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Group Name", "Group Name", "Group Name", GH_ParamAccess.item);
            pManager.AddGenericParameter("Group Parent", "Group Parent", "Group Parent", GH_ParamAccess.item);
            pManager[pManager.ParamCount-1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Group", "Group", "Group", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            string name = "";
            GH_ElementGroup parent = null;

            if (!DA.GetData(0, ref name))
                return;
            DA.GetData(1, ref parent);

            ElementGroupModel model = new ElementGroupModel(name);
            if(parent != null)
                model.GroupParent = parent.Value;

            DA.SetData(0, new GH_ElementGroup(model));
        }


        public override Guid ComponentGuid => new Guid("ad161a62-810b-4367-8979-e0e7725abb36");
    }
}
