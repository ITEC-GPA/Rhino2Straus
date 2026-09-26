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

public sealed class BeamDisplacementResultsComponent : SafeComponent
{
    public BeamDisplacementResultsComponent() : base("Beam Displacements", "Beam Displacements", "Beam Displacements", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("ea9c5153-c71b-5a9d-9cd8-2dee24da8b46");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Model path", "Model path", "Existing .st7 model path", GH_ParamAccess.item);
        p.AddTextParameter("Result path", "Result path", "Existing .LSA or compatible Straus7 result file", GH_ParamAccess.item);
        p.AddIntegerParameter("Case", "Case", "Result case number starting at 1", GH_ParamAccess.item, 1);
        p.AddIntegerParameter("Beam number", "Beam number", "Beam number", GH_ParamAccess.item, 1);
        p.AddIntegerParameter("Stations", "Stations", "Stations", GH_ParamAccess.item, 11);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddNumberParameter("Position", "Position", "Distance from end 1 in model length units", GH_ParamAccess.list);
        p.AddNumberParameter("Values", "Values", "One branch per station; column order is the Straus7 API order", GH_ParamAccess.tree);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var t=StrausFileService.BeamResults(Item<string>(da,0),Item<string>(da,1),Item<int>(da,2),Item<int>(da,3),St7.rtBeamDisp,St7.stBeamPrincipal,Item<int>(da,4));
        var tree=new GH_Structure<GH_Number>(); for(int i=0;i<t.Values.Length;i++)tree.AppendRange(t.Values[i].Select(x=>new GH_Number(x)),new GH_Path(i)); da.SetDataList(0,t.Positions); da.SetDataTree(1,tree);
    }
}
