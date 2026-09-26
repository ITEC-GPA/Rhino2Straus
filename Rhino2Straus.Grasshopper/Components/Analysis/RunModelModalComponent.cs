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

public sealed class RunModelModalComponent : SafeComponent
{
    private bool wasRun;private ModelModel solved;private string cachedKey="",status="Idle";
    public RunModelModalComponent() : base("Run Model Natural Frequencies", "Run Model Natural Frequencies", "Modal amplitudes follow Straus7 normalization; they are not displacements under a physical load case.", ComponentTopics.Tab, ComponentTopics.Analysis) { }
    public override Guid ComponentGuid => new Guid("7fdd9a12-3599-58dc-a1e5-7a49d58bcd53");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        p.AddTextParameter("Model path", "Model path", "Model path", GH_ParamAccess.item);
        p.AddBooleanParameter("Run", "Run", "Button or false-to-true transition", GH_ParamAccess.item, false);
        p.AddBooleanParameter("Overwrite", "Overwrite", "Overwrite", GH_ParamAccess.item, false);
        p.AddIntegerParameter("Modes", "Modes", "Modes", GH_ParamAccess.item, 10);
        p.AddNumberParameter("Shift Hz", "Shift Hz", "Shift Hz", GH_ParamAccess.item, 0.0);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        p.AddTextParameter("Result path", "Result path", "Result path", GH_ParamAccess.item);
        p.AddTextParameter("Status", "Status", "Status", GH_ParamAccess.item);
        p.AddTextParameter("Unavailable", "Unavailable", "Unavailable", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var input=Item<GH_Model>(da,0).Value;string path=Item<string>(da,1);bool run=Item<bool>(da,2),overwrite=Item<bool>(da,3);bool fire=run&&!wasRun;wasRun=run;
        string key=ModelFingerprint.Compute(input)+System.Text.Json.JsonSerializer.Serialize(new object[]{path,Item<int>(da,4),Item<double>(da,5)});
        if(key!=cachedKey){solved=null;status="Run required";}
        if(fire){solved=null;status="Failed";cachedKey=key;solved=ModelAnalysisService.RunModal(input,path,overwrite,Item<int>(da,4),Item<double>(da,5));status=solved.Results.IsComplete?"Completed":"Completed with unavailable quantities";}
        da.SetData(0,new GH_Model(solved??input));da.SetData(1,solved?.Results.ResultPath??"");da.SetData(2,status);da.SetDataList(3,solved?.Results.Issues.Select(x=>x.ToString())??Array.Empty<string>());Message=status;
        if(solved?.Results.Issues.Count>0)AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,$"{solved.Results.Issues.Count} result quantities unavailable; inspect Unavailable.");
    }
}
