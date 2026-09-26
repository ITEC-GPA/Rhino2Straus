using Rhino2Straus.Grasshopper;
using Grasshopper.Kernel;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Grasshopper.Datatype;
using System;

namespace Rhino2Straus.Grasshopper.Straus.Components.ElementProperties
{
    public class AreaThicknessComponent : SafeComponent
    {
        public AreaThicknessComponent()
            : base("Area thickness", "Area thickness", "Area thickness", ComponentTopics.Tab, ComponentTopics.Properties)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Thickness", "Thickness", "Thickness", GH_ParamAccess.item, 0.2);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Area thickness", "Area thickness", "Area thickness", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            string name = "";
            double thickness = 0.0;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref thickness))
            {
                AreaThicknessModel areaThickness = new AreaThicknessModel(name, thickness);
                DA.SetData(0, new GH_AreaThickness(areaThickness));
            }
        }


        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        public override Guid ComponentGuid => new Guid("fe66f2e0-1381-4c73-a206-f9f7e46346bc");
    }
}
