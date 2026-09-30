using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.ElementProperties;
using System.Linq;
using Rhino2Straus.Grasshopper;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2Straus.Grasshopper.Datatype;

namespace Rhino2Straus.Grasshopper.Straus.Components.Models
{
    public class FrameFilterComponent : SafeComponent
    {
        public FrameFilterComponent()
            : base("Frame Filter", "Frame Filter", "Frame Filter", ComponentTopics.Tab, ComponentTopics.Model)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame elements", "Frames", "Frames to filter. With no criteria, all frames pass. Supplied criteria are combined with OR.", GH_ParamAccess.list);
            pManager.AddPointParameter("Start Position", "Start Position", "Start Position", GH_ParamAccess.item);
            pManager.AddPointParameter("End Position", "End Position", "End Position", GH_ParamAccess.item);
            pManager.AddGenericParameter("Frame Properties", "Frame Properties", "Frame Properties", GH_ParamAccess.list);
            pManager.AddGenericParameter("Groups", "Groups", "Groups", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance", GH_ParamAccess.item, 0.001);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Filtered Frames", "Filtered Frames", "Filtered Frames", GH_ParamAccess.list);
            pManager.AddGenericParameter("Other Frames", "Other Frames", "Other Frames", GH_ParamAccess.list);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            var elements=new List<GH_FrameElement>();if(!DA.GetDataList(0,elements))return;
            Point3d start=Point3d.Unset;DA.GetData(1,ref start);
            Point3d end=Point3d.Unset;DA.GetData(2,ref end);
            var properties=new List<IGH_Goo>();DA.GetDataList(3,properties);
            var groups=new List<GH_ElementGroup>();DA.GetDataList(4,groups);
            double tolerance=Item<double>(DA,5);
            if(!double.IsFinite(tolerance)||tolerance<=0)throw new ArgumentException("Tolerance must be finite and positive.");
            var criteria=properties.Select(x=>x.ScriptVariable()).ToArray();
            if(criteria.Any(x=>x is not FramePropertyModel && x is not FrameSectionModel))throw new ArgumentException("Supply element properties (or legacy section definitions).");
            var groupNames=groups.Select(g=>g.Value.GroupFullName).ToHashSet();
            bool all=criteria.Length==0&&groupNames.Count==0&&!start.IsValid&&!end.IsValid;
            var matched=new List<GH_FrameElement>();var other=new List<GH_FrameElement>();
            foreach(var item in elements)
            {
                var value=item.Value;
                bool match=all||value.Groups.Any(g=>groupNames.Contains(g.GroupFullName))||(start.IsValid&&value.NodeStart.Position.DistanceTo(start)<=tolerance)||(end.IsValid&&value.NodeEnd.Position.DistanceTo(end)<=tolerance)
                    ||criteria.Any(p=>p is FramePropertyModel property?property.Name==value.FrameProperty.Name:((FrameSectionModel)p).Name==value.FrameSection.Name);
                (match?matched:other).Add(item);
            }
            DA.SetDataList(0,matched);DA.SetDataList(1,other);
        }

        public override Guid ComponentGuid => new Guid("38a772ad-451e-4af6-bead-52f530028b8b");
    }
}
