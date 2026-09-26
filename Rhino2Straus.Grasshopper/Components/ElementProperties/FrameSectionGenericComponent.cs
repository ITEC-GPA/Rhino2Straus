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
    public class FrameSectionGenericComponent : SafeComponent
    {
        public FrameSectionGenericComponent()
            : base("Frame Section Generic", "Frame Section Generic", "Frame Section Generic", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ELEMENTPROPERTIES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item, "Generic");
            pManager.AddNumberParameter("Area", "A", "Cross-sectional area in model length squared", GH_ParamAccess.item, 0.01);
            pManager.AddNumberParameter("I11", "I11", "Principal second moment, model length to the fourth", GH_ParamAccess.item, 8.333333333333334e-6);
            pManager.AddNumberParameter("I22", "I22", "Principal second moment, model length to the fourth", GH_ParamAccess.item, 8.333333333333334e-6);
            pManager.AddNumberParameter("J", "J", "Torsion constant, model length to the fourth", GH_ParamAccess.item, 1.406e-5);
            pManager.AddNumberParameter("Shear L1", "Shear L1", "Shear L1", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Shear L2", "Shear L2", "Shear L2", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Shear A1", "Shear A1", "Shear A1", GH_ParamAccess.item, 0.008333333333333333);
            pManager.AddNumberParameter("Shear A2", "Shear A2", "Shear A2", GH_ParamAccess.item, 0.008333333333333333);
            pManager.AddNumberParameter("Centroid x", "Centroid x", "Centroid x", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Centroid y", "Centroid y", "Centroid y", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Angle", "Angle", "Principal axis 1 angle in radians relative to section X", GH_ParamAccess.item, 0.0);
            pManager.AddBrepParameter("Profile","Profile","Optional planar XY face for preview only; centroid inputs locate its origin",GH_ParamAccess.item);pManager[12].Optional=true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame Section", "Frame Section", "Frame Section", GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            string name = "";
            double area = 0.0;
            double j11 = 0.0;
            double j22 = 0.0;
            double J = 0.0;
            double shearL1 = 0.0;
            double shearL2 = 0.0;
            double shearA1 = 0.0;
            double shearA2 = 0.0;
            double centroidX = 0.0;
            double CentroidY = 0.0;
            double angle = 0.0;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref area) && DA.GetData(2, ref j11) && DA.GetData(3, ref j22) && DA.GetData(4, ref J) && DA.GetData(5, ref shearL1) && DA.GetData(6, ref shearL2)
                && DA.GetData(7, ref shearA1) && DA.GetData(8, ref shearA2) && DA.GetData(9, ref centroidX) && DA.GetData(10, ref CentroidY) && DA.GetData(11, ref angle))
            {
                foreach(double value in new[]{area,j11,j22,J,shearL1,shearL2,shearA1,shearA2,centroidX,CentroidY,angle})
                    if(!double.IsFinite(value))throw new ArgumentException("Generic section data must be finite.");
                if(area<=0||j11<0||j22<0||J<0||shearL1<0||shearL2<0||shearA1<0||shearA2<0)
                    throw new ArgumentException("Area must be positive; inertia, torsion and shear values nonnegative.");
                FrameSectionModel frameProperty = new FrameSectionModel(name, FrameSectionModel.SectionGeometryTypes.Generic, FrameSectionModel.Types.DBUSER, FrameSectionModel.OffsetTypes.CC)
                {
                    SectionArea = area,
                    I11 = j11,
                    I22 = j22,
                    J = J,
                    ShearL1 = shearL1,
                    ShearL2 = shearL2,
                    ShearA1 = shearA1,
                    ShearA2 = shearA2,
                    Centroid = new Rhino.Geometry.Point2d(centroidX, CentroidY),
                    Angle = angle,
                };
                Rhino.Geometry.Brep profile=null;if(DA.GetData(12,ref profile)){var shape=profile.DuplicateBrep();if(shape.Faces.Count!=1||!shape.Faces[0].IsPlanar()||Math.Abs(shape.GetBoundingBox(true).Min.Z)>1e-8||Math.Abs(shape.GetBoundingBox(true).Max.Z)>1e-8)throw new ArgumentException("Profile must be one planar face in XY.");shape.Translate(-centroidX,-CentroidY,0);frameProperty.Breps=new(){shape};}
                DA.SetData(0, new GH_FrameSection(frameProperty));
            }
        }


        public override Guid ComponentGuid => new Guid("204d7ecc-d676-4b29-b2b2-8fa330afdf84");
    }
}
