using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2Straus.Grasshopper.Datatype;

namespace Rhino2Straus.Grasshopper.Straus.Components.Models
{
    public class AreaFilterComponent : SafeComponent
    {
        public AreaFilterComponent()
            : base("Area Filter", "Area Filter", "Area Filter", ComponentTopics.Tab, ComponentTopics.Model)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Area elements", "Area elements", "Plates to filter. With no criteria, all plates pass. Supplied criteria are combined with OR.", GH_ParamAccess.list);
            pManager.AddPointParameter("Position", "Position", "Position", GH_ParamAccess.item);
            pManager.AddGenericParameter("Area Properties", "Area Properties", "Area Properties", GH_ParamAccess.list);
            pManager.AddGenericParameter("Groups", "Groups", "Groups", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance", GH_ParamAccess.item, 0.001);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Filtered Areas", "Filtered Areas", "Filtered Areas", GH_ParamAccess.list);
            pManager.AddGenericParameter("Other Areas", "Other Areas", "Other Areas", GH_ParamAccess.list);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            var elements=new List<GH_AreaElement>();if(!DA.GetDataList(0,elements))return;
            Point3d start=Point3d.Unset;DA.GetData(1,ref start);
            var properties=new List<IGH_Goo>();DA.GetDataList(2,properties);
            var groups=new List<GH_ElementGroup>();DA.GetDataList(3,groups);
            double tolerance=Item<double>(DA,4);
            if(!double.IsFinite(tolerance)||tolerance<=0)throw new ArgumentException("Tolerance must be finite and positive.");
            var criteria=properties.Select(x=>x.ScriptVariable()).ToArray();
            if(criteria.Any(x=>x is not AreaPropertyModel && x is not AreaThicknessModel))throw new ArgumentException("Supply element properties (or legacy section definitions).");
            var groupNames=groups.Select(g=>g.Value.GroupFullName).ToHashSet();
            bool all=criteria.Length==0&&groupNames.Count==0&&!start.IsValid;
            var matched=new List<GH_AreaElement>();var other=new List<GH_AreaElement>();
            foreach(var item in elements)
            {
                var value=item.Value;
                bool match=all||value.Groups.Any(g=>groupNames.Contains(g.GroupFullName))||start.IsValid&&value.NodeList.Any(n=>n.Position.DistanceTo(start)<=tolerance)
                    ||criteria.Any(p=>p is AreaPropertyModel property?property.Name==value.AreaProperty.Name:((AreaThicknessModel)p).Name==value.AreaThickness.Name);
                (match?matched:other).Add(item);
            }
            DA.SetDataList(0,matched);DA.SetDataList(1,other);
        }

        public override Guid ComponentGuid => new Guid("58bbf0d8-9ace-4a27-b67f-0bf5e9bec547");
    }
}
