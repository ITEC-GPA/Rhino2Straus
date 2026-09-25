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
            : base("Area Filter", "Area Filter", "Area Filter", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_MODEL)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Area elements", "Area elements", "Area elements", GH_ParamAccess.list);
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
            List<GH_AreaElement> gh_Areas = new List<GH_AreaElement>();
            Point3d startNode = Point3d.Unset;
            List<GH_ElementGroup> gH_ElementGroups = new List<GH_ElementGroup>();
            List<GH_AreaThickness> gH_AreaProp = new List<GH_AreaThickness>();
            double tolerance = 0;

            int count = 0;
            if (!DA.GetDataList (count++, gh_Areas))
                return;
            DA.GetData(count++, ref startNode);
            DA.GetDataList(count++, gH_AreaProp);
            DA.GetDataList(count++, gH_ElementGroups);
            DA.GetData(count++, ref tolerance);

            List<GH_AreaElement> matchedAreas = new List<GH_AreaElement>();
            List<GH_AreaElement> otherAreas = new List<GH_AreaElement>();


            List<string> groupName = new List<string>();
            for (int i = 0; i < gH_ElementGroups.Count; i++)
                groupName.Add(gH_ElementGroups[i].Value.Name);
            List<string> propName = new List<string>();
            for (int i = 0; i < gH_AreaProp.Count; i++)
                propName.Add(gH_AreaProp[i].Value.Name);

            for (int i = 0; i < gh_Areas.Count; i++)
            {
                bool find = false;

                Core.Elements.AreaElementModel area = gh_Areas[i].Value;
                if (!find)
                {
                    for (int j = 0; j < area.Groups.Count; j++)
                        if (groupName.Contains(area.Groups[j].Name))
                            find = true;
                }

                if (!find)
                {
                    for(int j = 0; j < area.NodeList.Count; j++)
                        if (find! && area.NodeList[j].Position.DistanceTo(startNode) < tolerance)
                            find = true;
                }

                if (!find)
                {
                    if (propName.Contains(area.AreaThickness.Name))
                        find = true;
                }

                if (find)
                    matchedAreas.Add(gh_Areas[i]);
                else
                    otherAreas.Add(gh_Areas[i]);
            }

            DA.SetDataList(0, matchedAreas);
            DA.SetDataList(1, otherAreas);
        }


        public override Guid ComponentGuid => new Guid("58bbf0d8-9ace-4a27-b67f-0bf5e9bec547");
    }
}
