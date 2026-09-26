using Rhino2Straus.Grasshopper;
using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Loads;
using Rhino2Straus.Grasshopper.Datatype;

namespace Rhino2Straus.Grasshopper.Straus.Components.Loads
{
    public class LoadFrameUniformComponent : SafeComponent
    {
        public LoadFrameUniformComponent()
            : base("Load frame uniform", "Load frame uniform", "Load frame uniform", ComponentTopics.Tab, ComponentTopics.Loads)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame", "Frame", "Frame", GH_ParamAccess.item);
            pManager.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Type", "Type (Force or Moment)", "Type (Force or Moment)", GH_ParamAccess.item, 0);
            foreach (FrameLoadStrausModel.FrameLoadTypes v in Enum.GetValues(typeof(FrameLoadStrausModel.FrameLoadTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddIntegerParameter("Direction", "Direction", "Direction", GH_ParamAccess.item, 2);
            foreach (FrameLoadStrausModel.LoadDirections v in Enum.GetValues(typeof(FrameLoadStrausModel.LoadDirections)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddBooleanParameter("Projected?", "Projected?", "Bool", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Coordinate System", "Coordinate System", "Coordinate System", GH_ParamAccess.item, 1);
            Param_Integer dir_param = (Param_Integer)pManager[5];
            dir_param.AddNamedValue("Local", 0);
            dir_param.AddNamedValue("Global", 1);
            pManager.AddNumberParameter("Load", "Load", "Load", GH_ParamAccess.item, 0.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame", "Frame", "Frame", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            GH_FrameElement gH_FrameElement = null;
            GH_LoadCase loadCase = null;
            int type = 0;
            int direction = 0;
            bool isProjected = false;
            int coordinateSystem = 1; // Default to global coordinate system
            double num = 0.0;

            if (DA.GetData(0, ref gH_FrameElement) && DA.GetData(1, ref loadCase) && DA.GetData(2, ref type) &&
                DA.GetData(3, ref direction) && DA.GetData(4, ref isProjected) && DA.GetData(5, ref coordinateSystem) && DA.GetData(6, ref num))
            {
                FrameLoadStrausModel frameLoad = new FrameLoadStrausModel(loadCase.Value, (FrameLoadStrausModel.FrameLoadTypes)type, (FrameLoadStrausModel.LoadDirections)direction,
                    isProjected, 0.0, num, 1.0, num, FrameLoadStrausModel.LoadSchemas.Uniform, coordinateSystem == 1 ? CoordinateSystemModel.Global : CoordinateSystemModel.Local);
                FrameElementModel frameElementModel = new FrameElementModel(gH_FrameElement.Value);
                frameElementModel.FrameLoadList.Add(frameLoad);
                DA.SetData(0, new GH_FrameElement(frameElementModel));
            }
        }


        public override Guid ComponentGuid => new Guid("14bc0e1d-50ed-4a42-989f-08776a80d468");
    }
}
