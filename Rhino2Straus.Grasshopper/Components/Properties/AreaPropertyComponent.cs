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
    public class AreaPropertyComponent : SafeComponent
    {
        public AreaPropertyComponent()
            : base("Area Property", "Area Property", "Area Property", ComponentTopics.Tab, ComponentTopics.Properties)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item, "Plate");
            pManager.AddGenericParameter("Section", "Section", "Section", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "Material", "Material", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Type","Type","Native Straus7 plate property type",GH_ParamAccess.item,0);
            foreach(var type in Enum.GetValues<AreaPropertyModel.PlatePropertyType>())if(type!=AreaPropertyModel.PlatePropertyType.Undefined)((Param_Integer)pManager[3]).AddNamedValue(type.ToString(),(int)type);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Area Property", "Area Property", "Area Property", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            string name = "";
            GH_AreaThickness gH_AreaThickness= null;
            GH_Material gH_Material = null;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref gH_AreaThickness) && DA.GetData(2, ref gH_Material))
            {
                int type=0;DA.GetData(3,ref type);if(type<0||!Enum.IsDefined((AreaPropertyModel.PlatePropertyType)type))throw new ArgumentException("Invalid plate type.");
                AreaPropertyModel frameProperty = new AreaPropertyModel(name, gH_Material.Value, gH_AreaThickness.Value) { PropertyType = (AreaPropertyModel.PlatePropertyType)type };
                DA.SetData(0, new GH_AreaProperty(frameProperty));
            }
        }


        public override Guid ComponentGuid => new Guid("3edcae0b-6f75-48b9-94b2-538d8e714262");
    }
}
