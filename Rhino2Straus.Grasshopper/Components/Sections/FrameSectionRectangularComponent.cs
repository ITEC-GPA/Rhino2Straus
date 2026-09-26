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
    public class FrameSectionRectangularComponent : SafeComponent
    {
        public FrameSectionRectangularComponent()
            : base("Frame Section solid rectangular", "Frame Section solid rectangular", "Frame Section solid rectangular", ComponentTopics.Tab, ComponentTopics.Sections)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Height", "Height", "Height", GH_ParamAccess.item, 0.3);
            pManager.AddNumberParameter("Width", "Width", "Width", GH_ParamAccess.item, 0.2);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame Section", "Frame Section", "Frame Section", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            string name = "";
            double dimension = 0.0;
            double dimension2 = 0.0;
            if (DA.GetData(0, ref name) && DA.GetData(1, ref dimension) && DA.GetData(2, ref dimension2))
            {
                FrameSectionModel frameProperty = new FrameSectionModel(name, FrameSectionModel.SectionGeometryTypes.SB, FrameSectionModel.Types.DBUSER, FrameSectionModel.OffsetTypes.CC, dimension, dimension2);
                DA.SetData(0, new GH_FrameSection(frameProperty));
            }
        }


        public override Guid ComponentGuid => new Guid("c2eec84d-a85b-4596-b79f-44b8bad510bf");
    }
}
