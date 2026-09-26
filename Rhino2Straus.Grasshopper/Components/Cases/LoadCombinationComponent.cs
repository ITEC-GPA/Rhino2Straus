using Rhino2Straus.Grasshopper;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Grasshopper.Datatype;

namespace Rhino2Straus.Grasshopper.Straus.Components.Cases
{
    public class LoadCombinationComponent : SafeComponent
    {
        public LoadCombinationComponent()
            : base("Load combination", "Load combination", "Load combination", ComponentTopics.Tab, ComponentTopics.Cases)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item, "COMB1");
            pManager.AddGenericParameter("Load factor", "Load factor", "Load factor", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Type", "Type", "Type", GH_ParamAccess.item, 0);
            foreach (LoadCombinationModel.LoadCombinationTypes v in Enum.GetValues(typeof(LoadCombinationModel.LoadCombinationTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load combination", "Load combination", "Load combination", (GH_ParamAccess)0);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            string comboName = "";
            List<GH_LoadFactor> GH_loadFactors = new List<GH_LoadFactor>();
            int type = 0;
            if (!DA.GetData(0, ref comboName) || !DA.GetDataList(1, GH_loadFactors) || !DA.GetData(2, ref type))
                return;

            var loadFactors = new List<LoadFactorModel>();
            for (int i = 0; i < GH_loadFactors.Count; i++)
                loadFactors.Add(new LoadFactorModel(GH_loadFactors[i].Value));

            LoadCombinationModel loadCombination = new LoadCombinationModel(comboName, loadFactors, (LoadCombinationModel.LoadCombinationTypes)type);
            DA.SetData(0, new GH_LoadCombination(loadCombination));
        }


        public override Guid ComponentGuid => new Guid("1d5fa4d6-2bd8-4be9-978f-877280069ae6");
    }
}
