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
    public class FrameSectionIComponent : SafeComponent
    {
        public FrameSectionIComponent()
            : base("Frame Section I", "Frame Section I", "Frame Section I", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ELEMENTPROPERTIES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Height", "Height", "Height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width top flange", "Width top flange", "Width top flange", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness top flange", "Thickness top flange", "Thickness top flange", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width bottom flange", "Width bottom flange", "Width bottom flange", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness bottom flange", "Thickness", "Thickness", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness web", "Thickness web", "Thickness web", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radius inner", "Radius inner", "Radius inner", GH_ParamAccess.item, 0.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame Section", "Frame Section", "Frame Section", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            string name = "";
            double h = 0.0;
            double b1 = 0.0;
            double tw = 0.0;
            double tf1 = 0.0;
            double b2 = 0.0;
            double tf2 = 0.0;
            double r1 = 0.0;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref h) && DA.GetData(2, ref b1) && DA.GetData(3, ref tf1) &&
                DA.GetData(4, ref b2) && DA.GetData(5, ref tf2) && DA.GetData(6, ref tw) && DA.GetData(7, ref r1))
            {
                FrameSectionModel frameProperty = new FrameSectionModel(name, FrameSectionModel.SectionGeometryTypes.H, FrameSectionModel.Types.DBUSER, FrameSectionModel.OffsetTypes.CC, h, b1, tw, tf1, b2, tf2, r1);
                DA.SetData(0, new GH_FrameSection(frameProperty));
            }
        }


        public override Guid ComponentGuid => new Guid("2f0d4018-30a7-405d-a7d9-f32242a98ea0");
    }
}
