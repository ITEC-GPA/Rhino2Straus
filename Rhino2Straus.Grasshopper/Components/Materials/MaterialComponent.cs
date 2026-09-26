using Rhino2Straus.Grasshopper;

using System;

using Grasshopper.Kernel;

using Grasshopper.Kernel.Parameters;

using Rhino2Straus.Core.ElementProperties;

using Rhino2Straus.Core.Helper;

using Rhino2Straus.Grasshopper.Datatype;



namespace Rhino2Straus.Grasshopper.Straus.Components.Materials

{

    public class MaterialComponent : SafeComponent

    {

        public MaterialComponent()

            : base("Material", "Material", "Material", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_MATERIALS)

        {

        }



        protected override void RegisterInputParams(GH_InputParamManager pManager)

        {

            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item, "Steel");

            pManager.AddIntegerParameter("Type", "Type", "Steel or Concrete", GH_ParamAccess.item, 0);

            foreach (MaterialModel.MaterialTypes v in Enum.GetValues(typeof(MaterialModel.MaterialTypes)))

                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);

            pManager.AddNumberParameter("Modulus elasticity", "Modulus elasticity", "Young modulus in kPa", GH_ParamAccess.item, 2.1e8);

            pManager.AddNumberParameter("Poisson ratio", "Poisson ratio", "Poisson ratio", GH_ParamAccess.item, 0.3);

            pManager.AddNumberParameter("Thermal coefficent", "Thermal coefficent", "Thermal coefficent", GH_ParamAccess.item, 1.17E-05);

            pManager.AddNumberParameter("Density", "Density", "Mass density in tonne/m³", GH_ParamAccess.item, 7.849);

            pManager.AddNumberParameter("Damping ratio", "Damping", "Dimensionless damping ratio", GH_ParamAccess.item, 0.02);

        }



        protected override void RegisterOutputParams(GH_OutputParamManager pManager)

        {

            pManager.AddGenericParameter("Material", "Material", "Material", GH_ParamAccess.item);

        }



        protected override void Solve(IGH_DataAccess DA)

        {

            string name = Item<string>(DA, 0); int type = Item<int>(DA, 1);

            double e = Item<double>(DA, 2), poisson = Item<double>(DA, 3), alpha = Item<double>(DA, 4), density = Item<double>(DA, 5), damping = Item<double>(DA, 6);

            if (string.IsNullOrWhiteSpace(name) || !Enum.IsDefined(typeof(MaterialModel.MaterialTypes), type) || !double.IsFinite(e) || e <= 0 || !double.IsFinite(poisson) || poisson <= -1 || poisson >= 0.5 || !double.IsFinite(density) || density < 0 || !double.IsFinite(alpha) || !double.IsFinite(damping) || damping < 0)

                throw new ArgumentException("Invalid material values. E is in kPa and density is in tonne/m³.");

            DA.SetData(0, new GH_Material(new MaterialModel(name, (MaterialModel.MaterialTypes)type, damping, e, poisson, alpha, density, MaterialModel.Standards.Custom)));



        }





        public override Guid ComponentGuid => new Guid("1f804995-873c-44a5-b4b2-59e9f6966592");

    }

}
