using Rhino2Straus.Grasshopper;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Grasshopper.Datatype;

namespace Rhino2Straus.Grasshopper.Straus.Components.Materials
{
    public class DatabaseConcreteMaterialComponent : SafeComponent
    {
        protected Dictionary<int, string> _concreteList = new Dictionary<int, string>() {
            { 0, "C12/15" },
            { 1, "C16/20" },
            { 2, "C20/25" },
            { 3, "C25/30" },
            { 4, "C30/37" },
            { 5, "C35/45" },
            { 6, "C40/50" },
            { 7, "C45/55" },
            { 8, "C50/60" },
            { 9, "C55/67" },
            { 10, "C60/75" },
            { 11, "C70/85" },
            { 12, "C80/95" },
            { 13, "C90/105" }
        };

        public DatabaseConcreteMaterialComponent()
            : base("Database Concrete", "Database Concrete", "Database Concrete", Constants.CATEGORY_RHINO2STRAUS, Constants.SUBCATEGORY_MATERIALS)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            int i = pManager.AddIntegerParameter("Type", "T", "The property type", GH_ParamAccess.item, 4);
            Param_Integer mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _concreteList)
                mtParam.AddNamedValue(v.Value, v.Key);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Concrete Material", "Concrete Material", "Concrete Material", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            int type = 0;

            if (DA.GetData(0, ref type))
            {
                DA.SetData(0, new GH_Material(MaterialModel.ConcreteDatabase[_concreteList[type]]));
            }
        }


        public override Guid ComponentGuid => new Guid("221bab35-3cec-4d40-b75e-0c7dce2c7d11");
    }
}
