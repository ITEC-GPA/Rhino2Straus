using Rhino2Straus.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino2Straus.Grasshopper.Datatype;

namespace Rhino2Straus.Grasshopper.Straus.Components.Models
{
    public class DecomposeModelComponent : SafeComponent
    {
        public DecomposeModelComponent()
            : base("Decompose Model", "Decompose Model", "Decompose Model", ComponentTopics.Tab, ComponentTopics.Model)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Node elements", "Node elements", "Node elements", GH_ParamAccess.list);
            pManager.AddGenericParameter("Frame elements", "Frame elements", "Frame elements", GH_ParamAccess.list);
            pManager.AddGenericParameter("Area elements", "Area elements", "Area elements", GH_ParamAccess.list);
            pManager.AddGenericParameter("Link elements", "Link elements", "Link elements", GH_ParamAccess.list);
            pManager.AddGenericParameter("Solids","Solids","Solid elements",GH_ParamAccess.list);
            pManager.AddGenericParameter("Materials","Materials","Materials",GH_ParamAccess.list);
            pManager.AddGenericParameter("Beam sections","Beam sections","Beam sections",GH_ParamAccess.list);
            pManager.AddGenericParameter("Beam properties","Beam properties","Beam properties",GH_ParamAccess.list);
            pManager.AddGenericParameter("Plate properties","Plate properties","Plate properties",GH_ParamAccess.list);
            pManager.AddGenericParameter("Load cases","Load cases","Load cases",GH_ParamAccess.list);
            pManager.AddGenericParameter("Combinations","Combinations","Combinations",GH_ParamAccess.list);
            pManager.AddGenericParameter("Groups","Groups","Groups",GH_ParamAccess.list);
            pManager.AddGenericParameter("Units","Units","Units",GH_ParamAccess.item);
            pManager.AddGenericParameter("Results","Results","Results",GH_ParamAccess.item);
            pManager.AddGenericParameter("Self weight","SW","Gravity settings for rebuilding",GH_ParamAccess.item);
            pManager.AddBooleanParameter("Preserve nodes","Keep nodes","Connect to Build Model when rebuilding imported topology",GH_ParamAccess.item);
        }

        protected override void Solve(IGH_DataAccess DA)
        {
            GH_Model gH_Model = new GH_Model();
            int count = 0;
            if (!DA.GetData(count++, ref gH_Model))
                return;

            List<GH_NodeElement> gh_Nodes = new List<GH_NodeElement>(gH_Model.Value.NodeElements.Count);
            List<GH_FrameElement> gh_Frames = new List<GH_FrameElement>(gH_Model.Value.FrameElements.Count);
            List<GH_AreaElement> gH_Areas = new List<GH_AreaElement>(gH_Model.Value.AreaElements.Count);
            List<GH_LinkElement> gH_Links = new List<GH_LinkElement>(gH_Model.Value.LinkElements.Count);

            for (int i = 0; i < gH_Model.Value.NodeElements.Count; i++)
                gh_Nodes.Add(new GH_NodeElement(gH_Model.Value.NodeElements.ElementAt(i).Value));
            for (int i = 0; i < gH_Model.Value.FrameElements.Count; i++)
                gh_Frames.Add(new GH_FrameElement(gH_Model.Value.FrameElements.ElementAt(i).Value));
            for (int i = 0; i < gH_Model.Value.AreaElements.Count; i++)
                gH_Areas.Add(new GH_AreaElement(gH_Model.Value.AreaElements.ElementAt(i).Value));
            for (int i = 0; i < gH_Model.Value.LinkElements.Count; i++)
                gH_Links.Add(new GH_LinkElement(gH_Model.Value.LinkElements.ElementAt(i).Value));

            DA.SetDataList(0, gh_Nodes);
            DA.SetDataList(1, gh_Frames);
            DA.SetDataList(2, gH_Areas);
            DA.SetDataList(3, gH_Links);
            DA.SetDataList(4,gH_Model.Value.SolidElements.Values.Select(x=>new GH_SolidElement(x)));
            var model=gH_Model.Value;
            DA.SetDataList(5,model.Materials.Values.Select(x=>new GH_Material(x)));
            DA.SetDataList(6,model.FrameSections.Values.Select(x=>new GH_FrameSection(x)));
            DA.SetDataList(7,model.FrameProperties.Values.Select(x=>new GH_FrameProperty(x)));
            DA.SetDataList(8,model.AreaProperties.Values.Select(x=>new GH_AreaProperty(x)));
            DA.SetDataList(9,model.LoadCases.Values.Select(x=>new GH_LoadCase(x)));
            DA.SetDataList(10,model.LoadCombinations.Values.Select(x=>new GH_LoadCombination(x)));
            DA.SetDataList(11,model.Groups.Values.Select(x=>new GH_ElementGroup(x)));
            DA.SetData(12,new GH_Units(model.ModelUnits));
            if(model.SelfWeight!=null)DA.SetData(14,new GH_SelfWeight(model.SelfWeight));
            DA.SetData(15,model.PreserveCoincidentNodes);
            if(model.Results!=null){model.Results.ValidateFor(model);DA.SetData(13,new global::Grasshopper.Kernel.Types.GH_ObjectWrapper(model.Results));}

        }


        public override Guid ComponentGuid => new Guid("db47b5d0-3d45-48a6-82f4-cea65426e9a6");
    }
}
