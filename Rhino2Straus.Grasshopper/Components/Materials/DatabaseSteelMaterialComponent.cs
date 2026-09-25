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
    public class DatabaseSteelMaterialComponent : SafeComponent
    {
        protected Dictionary<int, string> _steelList = new Dictionary<int, string>() {
            { 0, "S235" },
            { 1, "S275" },
            { 2, "S355" },
            { 3, "S450" },
        };

        public DatabaseSteelMaterialComponent()
            : base("Database Steel", "Database Steel", "Database Steel", Constants.CATEGORY_RHINO2STRAUS, Constants.SUBCATEGORY_MATERIALS)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            int i = pManager.AddIntegerParameter("Type", "T", "The property type", GH_ParamAccess.item);
            Param_Integer mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _steelList)
                mtParam.AddNamedValue(v.Value, v.Key);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Steel Material", "Steel Material", "Steel Material", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            int type = 0;

            if (DA.GetData(0, ref type))
            {
                DA.SetData(0, new GH_Material(MaterialModel.SteelDatabase[_steelList[type]]));
            }
        }


        public override Guid ComponentGuid => new Guid("73672cff-9268-4de0-b752-1bafafb4d1b5");
    }
}
