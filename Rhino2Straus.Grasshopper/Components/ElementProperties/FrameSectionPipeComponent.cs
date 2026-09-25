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
    public class FrameSectionPipeComponent : SafeComponent
    {
        public FrameSectionPipeComponent()
            : base("Frame Section pipe", "Frame Section pipe", "Frame Section pipe", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ELEMENTPROPERTIES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Diameter", "Diameter", "Diameter", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness", "Thickness", "Thickness", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame property", "Frame property", "Frame property", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            string name = "";
            double dimension = 0.0;
            double dimension2 = 0.0;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref dimension) && DA.GetData(2, ref dimension2))
            {
                FrameSectionModel frameProperty = new FrameSectionModel(name, FrameSectionModel.SectionGeometryTypes.P, FrameSectionModel.Types.DBUSER, FrameSectionModel.OffsetTypes.CC,
                    dimension, dimension2);
                DA.SetData(0, new GH_FrameSection(frameProperty));
            }
        }


        public override Guid ComponentGuid => new Guid("dc51835d-9673-4fed-948b-2ec5eb66deaf");
    }
}
