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

public sealed class MergeModelsComponent : SafeComponent
{
    public MergeModelsComponent() : base("Merge Straus Models", "Merge Straus Models", "Merge Straus Models", ComponentTopics.Tab, ComponentTopics.Model) { }
    public override Guid ComponentGuid => new Guid("356a6055-5867-5789-bdd1-c4e1d16e4a2a");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Models", "Models", "Models", GH_ParamAccess.list);
        p.AddBooleanParameter("Renumber", "Renumber", "Renumber independent parts. Nodes are welded for created models; importing a Model preserves distinct numbered nodes throughout the merge", GH_ParamAccess.item, false);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var models=new List<GH_Model>();da.GetDataList(0,models);da.SetData(0,new GH_Model(ModelComposer.Merge(models.Select(x=>x.Value),Item<bool>(da,1))));
    }
}
