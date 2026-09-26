using Rhino2Straus.Grasshopper;
using Grasshopper.Kernel;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Grasshopper.Datatype;
using System;

namespace Rhino2Straus.Grasshopper.Straus.Components.ElementProperties
{
    public class FrameSectionAngleComponent : SafeComponent
    {
        public FrameSectionAngleComponent()
            : base("Frame Section Angle", "Frame Section Angle", "Frame Section Angle", ComponentTopics.Tab, ComponentTopics.Sections)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Height", "Height", "Height", GH_ParamAccess.item, 0.3);
            pManager.AddNumberParameter("Width", "Width", "Width", GH_ParamAccess.item, 0.2);
            pManager.AddNumberParameter("Thickness web", "Thickness web", "Thickness web", GH_ParamAccess.item, 0.01);
            pManager.AddNumberParameter("Thickness flange", "Thickness flange", "Thickness flange", GH_ParamAccess.item, 0.01);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame property", "Frame property", "Frame property", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            string name = "";
            double h = 0.0;
            double w = 0.0;
            double tw = 0.0;
            double tf = 0.0;
            double dimension5 = 0.0;
            double dimension6 = 0.0;
            double dimension7 = 0.0;
            double dimension8 = 0.0;
            double dimension9 = 0.0;
            double dimension10 = 0.0;

            int count = 0;
            if (!DA.GetData(count++, ref name))
                return;
            if (!DA.GetData(count++, ref h))
                return;
            if (!DA.GetData(count++, ref w))
                return;
            if (!DA.GetData(count++, ref tw))
                return;
            if (!DA.GetData(count++, ref tf))
                return;

            FrameSectionModel frameProperty = new FrameSectionModel(name, FrameSectionModel.SectionGeometryTypes.L, FrameSectionModel.Types.DBUSER, FrameSectionModel.OffsetTypes.CC,
                h, w, tw, tf, dimension5, dimension6, dimension7, dimension8, dimension9, dimension10);
            DA.SetData(0, new GH_FrameSection(frameProperty));
        }


        public override Guid ComponentGuid => new Guid("96a3255f-e896-4403-915f-a846f156643b");
    }
}
