using Rhino2Straus.Grasshopper;
using System;
using Grasshopper.Kernel;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.Loads;
using Rhino2Straus.Grasshopper.Datatype;

namespace Rhino2Straus.Grasshopper.Straus.Components.Loads
{
    public class LoadNodalComponent : SafeComponent
    {
        public LoadNodalComponent()
            : base("Load nodal", "Load nodal", "Load nodal", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_LOADS)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
            pManager.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
            pManager.AddNumberParameter("Fx", "Fx", "Fx", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Fy", "Fy", "Fy", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Fz", "Fz", "Fz", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Mx", "Mx", "Mx", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("My", "My", "My", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Mz", "Mz", "Mz", GH_ParamAccess.item, 0.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            GH_NodeElement gH_Node = null;
            GH_LoadCase gH_LoadCase = null;
            double fX = 0.0;
            double fY = 0.0;
            double fZ = 0.0;
            double mX = 0.0;
            double mY = 0.0;
            double mZ = 0.0;

            if (DA.GetData(0, ref gH_Node) && DA.GetData(1, ref gH_LoadCase) && DA.GetData(2, ref fX) && DA.GetData(3, ref fY) && DA.GetData(4, ref fZ) &&
                DA.GetData(5, ref mX) && DA.GetData(6, ref mY) && DA.GetData(7, ref mZ))
            {
                NodalLoadModel nodalLoad = new NodalLoadModel(gH_LoadCase.Value, fX, fY, fZ, mX, mY, mZ);
                NodeElementModel newnode = new NodeElementModel(gH_Node.Value);
                newnode.NodalLoadList.Add(nodalLoad);
                DA.SetData(0, new GH_NodeElement(newnode));
            }
        }


        public override Guid ComponentGuid => new Guid("cf708797-8d98-4bbe-a746-11baf38ea456");
    }
}
