using Rhino2Straus.Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Grasshopper.Datatype;
using System;

namespace Rhino2Straus.Grasshopper.Straus.Components.Attributes
{
    public class LinkPropertyComponent : SafeComponent
    {
        public LinkPropertyComponent()
            : base("Link Type", "Link Type", "Link Type", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ATTRIBUTES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Type", "Type", "0=Rigid, 1=Elastic connection, 2=Pinned, 3=Master-slave, 4=Sector symmetry", GH_ParamAccess.item, 0);
            foreach (LinkPropertyModel.LinkPropertyTypes v in Enum.GetValues(typeof(LinkPropertyModel.LinkPropertyTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddNumberParameter("Kx", "Kx", "Kx", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Ky", "Ky", "Ky", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Kz", "Kz", "Kz", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Rx", "Rx", "Rx", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Ry", "Ry", "Ry", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Rz", "Rz", "Rz", GH_ParamAccess.item, 0);
            pManager.AddGenericParameter("Options","Options","Link Constraint Options, optional; all axes global XYZ",GH_ParamAccess.item);pManager[7].Optional=true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Link Type", "Link Type", "Link Type", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            int type = 0;
            double dx = 0;
            double dy = 0;
            double dz = 0;
            double mx = 0;
            double my = 0;
            double mz = 0;

            if (!DA.GetData(0, ref type))
                return;

            if (DA.GetData(1, ref dx) && DA.GetData(2, ref dy) && DA.GetData(3, ref dz) && DA.GetData(4, ref mx) && DA.GetData(5, ref my) && DA.GetData(6, ref mz))
            {
                LinkPropertyModel linkProperty = new LinkPropertyModel((LinkPropertyModel.LinkPropertyTypes)type, dx, dy, dz, mx, my, mz);
                if(!Enum.IsDefined(linkProperty.Type))throw new ArgumentException("Unsupported link type.");
                foreach(double stiffness in new[]{dx,dy,dz,mx,my,mz})if(!double.IsFinite(stiffness)||stiffness<0)throw new ArgumentException("Link stiffness must be finite and nonnegative.");
                LinkConstraintOptions options=null;DA.GetData(7,ref options);linkProperty.Options=(options??new()).Copy();linkProperty.Options.Validate();
                DA.SetData(0, new GH_LinkProperty(linkProperty));
            }
        }


        public override Guid ComponentGuid => new Guid("68b40983-74b8-43fa-91f1-4ed76e73a0e2");

        public override GH_Exposure Exposure => GH_Exposure.quarternary ;
    }
}
