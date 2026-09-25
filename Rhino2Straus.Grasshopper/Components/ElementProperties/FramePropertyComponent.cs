using Rhino2Straus.Grasshopper;
using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Straus.Core.Base;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Grasshopper.Datatype;

namespace Rhino2Straus.Grasshopper.Straus.Components.ElementProperties
{
    public class FramePropertyComponent : SafeComponent
    {
        public FramePropertyComponent()
            : base("Frame Property", "Frame Property", "Frame Property", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ELEMENTPROPERTIES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddGenericParameter("Section", "Section", "Section", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "Material", "Material", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame Property", "Frame Property", "Frame Property", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            string name = "";
            GH_FrameSection gH_FrameSection = null;
            GH_Material gH_Material = null;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref gH_FrameSection) && DA.GetData(2, ref gH_Material))
            {
                FramePropertyModel frameProperty = new FramePropertyModel(name, gH_Material.Value, gH_FrameSection.Value);
                DA.SetData(0, new GH_FrameProperty(frameProperty));
            }
        }


        public override Guid ComponentGuid => new Guid("7e49f69a-f7d7-446f-8fd0-76b9b4263890");
    }
}
