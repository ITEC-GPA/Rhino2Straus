using Rhino2Straus.Grasshopper;
using Grasshopper.Kernel;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Grasshopper.Datatype;
using System;

namespace Rhino2Straus.Grasshopper.Straus.Components.ElementProperties
{
    public class FrameSectionChannelComponent : SafeComponent
    {
        public FrameSectionChannelComponent()
            : base("Frame Section channel", "Frame Section channel", "Frame Section channel", ComponentTopics.Tab, ComponentTopics.Sections)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Height", "Height", "Height", GH_ParamAccess.item, 0.3);
            pManager.AddNumberParameter("Width top flange", "Width top flange", "Width top flange", GH_ParamAccess.item, 0.2);
            pManager.AddNumberParameter("Thickness web", "Thickness web", "Thickness web", GH_ParamAccess.item, 0.01);
            pManager.AddNumberParameter("Thickness top flange", "Thickness top flange", "Thickness top flange", GH_ParamAccess.item, 0.01);
            pManager.AddNumberParameter("Width bottom flange", "Width bottom flange", "Width bottom flange", GH_ParamAccess.item, 0.2);
            pManager.AddNumberParameter("Thickness bottom flange", "Thickness", "Thickness", GH_ParamAccess.item, 0.01);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame property", "Frame property", "Frame property", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            string name = "";
            double h = 0.0;
            double topFlangeWidth = 0.0;
            double webThick = 0.0;
            double topFlangeThick = 0.0;
            double bottomFlangeWidth = 0.0;
            double bottomFlangeThick = 0.0;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref h) && DA.GetData(2, ref topFlangeWidth) &&
                DA.GetData(3, ref webThick) && DA.GetData(4, ref topFlangeThick) && DA.GetData(5, ref bottomFlangeWidth) && DA.GetData(6, ref bottomFlangeThick))
            {
                FrameSectionModel frameProperty = new FrameSectionModel(name, FrameSectionModel.SectionGeometryTypes.C, FrameSectionModel.Types.DBUSER, FrameSectionModel.OffsetTypes.CC,
                    h, topFlangeWidth, webThick, topFlangeThick, bottomFlangeWidth, bottomFlangeThick);
                DA.SetData(0, new GH_FrameSection(frameProperty));
            }
        }


        public override Guid ComponentGuid => new Guid("2615abe4-fea4-4fad-9693-89174d6e3347");
    }
}
