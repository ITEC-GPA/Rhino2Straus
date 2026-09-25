using Rhino2Straus.Grasshopper;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Base;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Grasshopper.Datatype;

namespace Rhino2Straus.Grasshopper.Straus.Components.Elements
{
    public class AreaElementComponent : SafeComponent
    {

        public AreaElementComponent()
            : base("Area element", "Area element", "Area element", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ELEMENTS)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Area", "Area", "Area", GH_ParamAccess.item);
            pManager.AddGenericParameter("Area Property", "Area Property", "Area Property", GH_ParamAccess.item);
            pManager.AddAngleParameter("Angle (deg)", "Angle (deg)", "Angle (deg)", GH_ParamAccess.item, 0);
            pManager.AddGenericParameter("Groups", "Groups", "Groups", GH_ParamAccess.list);
            ((GH_ParamManager)pManager)[3].Optional = true;
            pManager.AddNumberParameter("Offset", "Offset", "Offset", GH_ParamAccess.item, 0.0);
            ((GH_ParamManager)pManager)[4].Optional = true;
            pManager.AddIntegerParameter("Area ID", "Area ID", "Area ID", GH_ParamAccess.item, ModelObjectId.UNASSIGNED);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Area element", "Area element", "Area element", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            GH_AreaProperty areaProp = new GH_AreaProperty();
            List<IGH_Goo> list = new List<IGH_Goo>();
            double angle = 0.0;
            double offset = 0.0;
            List<GH_ElementGroup> groups = new List<GH_ElementGroup>();
            int id = 0;

            if (!DA.GetData(0, ref mesh) || !DA.GetData(1, ref areaProp))
                return;
            DA.GetData(2, ref angle);
            DA.GetDataList(3, groups);
            DA.GetData(4, ref offset);
            DA.GetData(5, ref id);

            if(mesh.Faces.Count!=1)throw new ArgumentException("Supply one Tri3/Quad4 face. Use Mesh To Plates for multiple faces.");
            var face=mesh.Faces[0];var indices=face.IsTriangle?new[]{face.A,face.B,face.C}:new[]{face.A,face.B,face.C,face.D};
            var listBuffer=new List<NodeElementModel>();foreach(int index in indices)listBuffer.Add(new NodeElementModel(mesh.Vertices[index]));

            AreaElementModel areaElement = new AreaElementModel(listBuffer, areaProp.Value, Rhino.RhinoMath.ToRadians(angle), offset) { Id = id };
            if (groups != null && groups.Count > 0)
            {
                List<ElementGroupModel> elementGroupModels = new List<ElementGroupModel>();
                for (int i = 0; i < groups.Count; i++)
                    elementGroupModels.Add(groups[i].Value);

                areaElement.Groups = elementGroupModels;
            }

            DA.SetData(0, new GH_AreaElement(areaElement));
        }


        public override Guid ComponentGuid => new Guid("98982095-f828-4deb-a0fc-e18a5949b5e7");
    }
}
