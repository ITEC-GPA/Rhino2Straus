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

public sealed class RunModelCasesComponent : SafeComponent
{
    private bool wasRun; private ModelModel solved; private string cachedKey="",status="Idle";
    public RunModelCasesComponent() : base("Run Model Cases and Combinations", "Run Model Cases and Combinations", "Export this Model, run selected linear static cases and combinations, and return the Model with results and provenance. Requires a Straus7 API/solver licence.", "Rhino2Straus", "11-Analysis") { }
    public override Guid ComponentGuid => new Guid("4a573019-35cb-55d7-8580-99ad7613003b");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        p.AddTextParameter("Model path", "Model path", "Output .st7 path", GH_ParamAccess.item);
        p.AddBooleanParameter("Run", "Run", "Button or false-to-true transition", GH_ParamAccess.item, false);
        p.AddBooleanParameter("Overwrite", "Overwrite", "Overwrite", GH_ParamAccess.item, false);
        p.AddTextParameter("Case names", "Case names", "Empty means all enabled cases; combo dependencies are included", GH_ParamAccess.list);
        p[4].Optional = true;
        p.AddTextParameter("Combination names", "Combination names", "Empty means all enabled combinations", GH_ParamAccess.list);
        p[5].Optional = true;
        p.AddBooleanParameter("Run cases", "Run cases", "Run cases", GH_ParamAccess.item, true);
        p.AddBooleanParameter("Run combinations", "Run combinations", "Run combinations", GH_ParamAccess.item, true);
        p.AddIntegerParameter("Stations", "Stations", "Minimum beam stations 2..100; Straus7 may insert extrema", GH_ParamAccess.item, 11);
        p.AddIntegerParameter("Sample", "Sample", "0 centroid; 1 Gauss; 2 nodes unaveraged; 3 all; 4 same property", GH_ParamAccess.item, 1);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Assembled Model including immutable results", GH_ParamAccess.item);
        p.AddTextParameter("Result path", "Result path", "Result path", GH_ParamAccess.item);
        p.AddTextParameter("Status", "Status", "Status", GH_ParamAccess.item);
        p.AddTextParameter("Unavailable", "Unavailable", "Unavailable quantities with case and element identity", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var input=Item<GH_Model>(da,0).Value;bool run=Item<bool>(da,2);bool fire=run&&!wasRun;wasRun=run;
        var cases=new List<string>();var combos=new List<string>();da.GetDataList(4,cases);da.GetDataList(5,combos);
        string key=ModelFingerprint.Compute(input)+System.Text.Json.JsonSerializer.Serialize(new object[]{Item<string>(da,1),cases,combos,Item<bool>(da,6),Item<bool>(da,7),Item<int>(da,8),Item<int>(da,9)});
        if(key!=cachedKey){solved=null;status="Inputs changed - run required";}
        if(fire){solved=null;status="Failed";cachedKey=key;solved=ModelAnalysisService.RunStatic(input,Item<string>(da,1),Item<bool>(da,3),cases,combos,Item<bool>(da,6),Item<bool>(da,7),Item<int>(da,8),Item<int>(da,9));status=solved.Results.IsComplete?"Completed":"Completed with unavailable quantities";}
        var output=solved??input;da.SetData(0,new GH_Model(output));da.SetData(1,solved?.Results.ResultPath??"");da.SetData(2,status);
        da.SetDataList(3,solved?.Results.Issues.Select(x=>x.ToString())??Array.Empty<string>());Message=status;
        if(solved?.Results.Issues.Count>0)AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,$"{solved.Results.Issues.Count} result quantities unavailable. Inspect Unavailable before verification.");
    }
}
