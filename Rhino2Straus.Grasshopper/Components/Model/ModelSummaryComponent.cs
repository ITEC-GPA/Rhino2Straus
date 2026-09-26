using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Loads;
using Rhino2Straus.Core.Models;
using Rhino2Straus.Core.Settings;
using Rhino2Straus.Grasshopper.Datatype;
using Rhino2Straus.Import;
using St7API;

namespace Rhino2Straus.Grasshopper.Components.Extended;

public sealed class ModelSummaryComponent : SafeComponent
{
    public ModelSummaryComponent() : base("Model Summary", "Model Summary", "Model Summary", ComponentTopics.Tab, ComponentTopics.Model) { }
    public override Guid ComponentGuid => new Guid("53689eef-1768-56b4-ad15-d4b22653b7ef");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddIntegerParameter("Nodes", "Nodes", "Nodes", GH_ParamAccess.item);
        p.AddIntegerParameter("Beams", "Beams", "Beams", GH_ParamAccess.item);
        p.AddIntegerParameter("Plates", "Plates", "Plates", GH_ParamAccess.item);
        p.AddIntegerParameter("Links", "Links", "Links", GH_ParamAccess.item);
        p.AddTextParameter("Load cases", "Load cases", "Load cases", GH_ParamAccess.list);
        p.AddTextParameter("Combinations", "Combinations", "Combinations", GH_ParamAccess.list);
        p.AddTextParameter("Groups", "Groups", "Groups", GH_ParamAccess.list);
        p.AddIntegerParameter("Solids", "Solids", "Solid elements", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var m=Item<GH_Model>(da,0).Value; da.SetData(0,m.NodeElements.Count); da.SetData(1,m.FrameElements.Count); da.SetData(2,m.AreaElements.Count); da.SetData(3,m.LinkElements.Count);
        da.SetDataList(4,m.LoadCases.Keys); da.SetDataList(5,m.LoadCombinations.Keys); da.SetDataList(6,m.Groups.Keys);
        da.SetData(7,m.SolidElements.Count);
    }
}
