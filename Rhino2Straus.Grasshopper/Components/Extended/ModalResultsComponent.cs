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

public sealed class ModalResultsComponent : SafeComponent
{
    public ModalResultsComponent() : base("Modal Frequencies", "Modal Frequencies", "Modal Frequencies", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("dc77f972-0b6f-5cdc-9edd-579c8c641ad9");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Model path", "Model path", "Model path", GH_ParamAccess.item);
        p.AddTextParameter("Result path", "Result path", "Result path", GH_ParamAccess.item);
        p.AddIntegerParameter("Modes", "Modes", "Modes", GH_ParamAccess.list);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddNumberParameter("Frequency Hz", "Frequency Hz", "Frequency Hz", GH_ParamAccess.list);
        p.AddNumberParameter("Modal mass", "Modal mass", "Modal mass", GH_ParamAccess.list);
        p.AddNumberParameter("Values", "Values", "Branches by mode: frequency, mass, stiffness, damping, translational and rotational mass and mode participation", GH_ParamAccess.tree);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var modes=new List<int>();da.GetDataList(2,modes);var values=StrausFileService.ModalResults(Item<string>(da,0),Item<string>(da,1),modes.ToArray());
        da.SetDataList(0,values.Select(v=>v[0]));da.SetDataList(1,values.Select(v=>v[1]));var tree=new DataTree<double>();for(int i=0;i<values.Length;i++)tree.AddRange(values[i],new GH_Path(modes[i]));da.SetDataTree(2,tree);
    }
}
