using Rhino2Straus.Grasshopper;
using Grasshopper.Kernel;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Grasshopper.Datatype;
using System;

namespace Rhino2Straus.Grasshopper.Straus.Components.Cases
{
    public class LoadCaseComponent : SafeComponent
    {
        public LoadCaseComponent()
            : base("Load case", "Load case", "Load case", ComponentTopics.Tab, ComponentTopics.Cases)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item, "LC1");
            pManager.AddBooleanParameter("Gravity NSM", "NSM", "Apply gravity to non-structural mass", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Reference temperature","T ref","Optional absolute reference temperature in Model units; unwired uses Straus default",GH_ParamAccess.item);pManager[2].Optional=true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            string name = "";
            bool nsm = false;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref nsm))
            {
                LoadCaseModel loadCase = new LoadCaseModel(name, LoadCaseModel.LoadCaseTypes.USER, "", nsm);
                double reference=0;if(DA.GetData(2,ref reference))loadCase.ReferenceTemperature=reference;
                DA.SetData(0, new GH_LoadCase(loadCase));
            }
        }


        public override Guid ComponentGuid => new Guid("65f6274a-354a-4965-a40e-73af273912fc");
    }
}
