using Rhino2Straus.Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Loads;
using Rhino2Straus.Grasshopper.Datatype;
using System;

namespace Rhino2Straus.Grasshopper.Straus.Components.Loads
{
    public class LoadFrameNonUniComponent : SafeComponent
    {
        public LoadFrameNonUniComponent()
            : base("Load frame non-uniform", "Load frame non-uniform", "Load frame non-uniform", ComponentTopics.Tab, ComponentTopics.Loads)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame", "Frame", "Frame", GH_ParamAccess.item);
            pManager.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Type", "Type (Force or Moment)", "Type (Force or Moment)", GH_ParamAccess.item, 0);
            foreach (FrameLoadStrausModel.FrameLoadTypes v in Enum.GetValues(typeof(FrameLoadStrausModel.FrameLoadTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddIntegerParameter("Direction", "Direction", "Lx/Ly/Lz/Gx/Gy/Gz", GH_ParamAccess.item, 5);
            foreach (FrameLoadStrausModel.LoadDirections v in Enum.GetValues(typeof(FrameLoadStrausModel.LoadDirections)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddBooleanParameter("Projected?", "Projected?", "Bool", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Coordinate System", "Coordinate System", "Coordinate System", GH_ParamAccess.item, 1);
            Param_Integer dir_param = (Param_Integer)pManager[5];
            dir_param.AddNamedValue("Local", 0);
            dir_param.AddNamedValue("Global", 1);
            pManager.AddNumberParameter("Load at start", "Load at start", "Load at start", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Load at end", "Load at end", "Load at end", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Location relative start", "Location relative start", "Location relative start", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Location relative end", "Location relative end", "Location relative end", GH_ParamAccess.item, 1.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame load", "Frame load", "Frame load", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            GH_FrameElement gH_FrameElement = null;
            GH_LoadCase loadCase = null;
            int type = 0;
            int direction = 0;
            bool isProjected = false;
            int coordinateSystem = 1; // Default to global coordinate system
            double startLoad = 0.0;
            double endLoad = 0.0;
            double num = 0.0;
            double num2 = 0.0;

            if (DA.GetData(0, ref gH_FrameElement) && DA.GetData(1, ref loadCase) && DA.GetData(2, ref type) &&
                DA.GetData(3, ref direction) && DA.GetData(4, ref isProjected) && DA.GetData(5, ref coordinateSystem) && DA.GetData(6, ref startLoad) &&
                DA.GetData(7, ref endLoad) && DA.GetData(8, ref num) && DA.GetData(9, ref num2))
            {
                if (num >= num2)
                {
                    AddRuntimeMessage((GH_RuntimeMessageLevel)20, "Location start must be > location end!");
                    return;
                }
                if (num < 0.0 || num2 < 0.0)
                {
                    AddRuntimeMessage((GH_RuntimeMessageLevel)20, "Location start and end must be >= 0!");
                    return;
                }

                FrameElementModel frameElementModel = new FrameElementModel(gH_FrameElement.Value);
                FrameLoadStrausModel frameLoad = new FrameLoadStrausModel(loadCase.Value, (FrameLoadStrausModel.FrameLoadTypes)type, (FrameLoadStrausModel.LoadDirections)direction,
                    isProjected, num, startLoad, num2, endLoad, FrameLoadStrausModel.LoadSchemas.Keystone, coordinateSystem == 1 ? CoordinateSystemModel.Global : CoordinateSystemModel.Local);
                frameElementModel.FrameLoadList.Add(frameLoad);
                DA.SetData(0, new GH_FrameElement(frameElementModel));
            }
        }


        public override Guid ComponentGuid => new Guid("cd36a3ea-5037-45a7-953f-0fc70d639e58");
    }
}
