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

public sealed class RunModelBucklingComponent : SafeComponent
{
    private bool wasRun;private ModelModel solved;private string cachedKey="",status="Idle";
    public RunModelBucklingComponent() : base("Run Model Linear Buckling", "Run Model Linear Buckling", "Buckling using verified initial static results from the same Model; modal displacements are normalized mode shapes.", ComponentTopics.Tab, ComponentTopics.Analysis) { }
    public override Guid ComponentGuid => new Guid("323cda05-f8cf-506e-bba6-af56a2d89dde");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        p.AddTextParameter("Model path", "Model path", "Model path", GH_ParamAccess.item);
        p.AddBooleanParameter("Run", "Run", "Button or false-to-true transition", GH_ParamAccess.item, false);
        p.AddBooleanParameter("Overwrite", "Overwrite", "Overwrite", GH_ParamAccess.item, false);
        p.AddTextParameter("Initial LSA", "Initial LSA", "Verified LSA from this Model", GH_ParamAccess.item);
        p.AddIntegerParameter("Variable result case", "Variable result case", "Variable result case", GH_ParamAccess.item, 1);
        p.AddIntegerParameter("Fixed result case", "Fixed result case", "0 means none", GH_ParamAccess.item, 0);
        p.AddIntegerParameter("Modes", "Modes", "Modes", GH_ParamAccess.item, 10);
        p.AddNumberParameter("Shift", "Shift", "Shift", GH_ParamAccess.item, 0.0);
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
        string key=ModelFingerprint.Compute(input)+System.Text.Json.JsonSerializer.Serialize(new object[]{path,Item<string>(da,4),Item<int>(da,5),Item<int>(da,6),Item<int>(da,7),Item<double>(da,8)});
        if(key!=cachedKey){solved=null;status="Run required";}
        if(fire){solved=null;status="Failed";cachedKey=key;solved=ModelAnalysisService.RunBuckling(input,path,overwrite,Item<string>(da,4),Item<int>(da,5),Item<int>(da,6),Item<int>(da,7),Item<double>(da,8));status=solved.Results.IsComplete?"Completed":"Completed with unavailable quantities";}
        da.SetData(0,new GH_Model(solved??input));da.SetData(1,solved?.Results.ResultPath??"");da.SetData(2,status);da.SetDataList(3,solved?.Results.Issues.Select(x=>x.ToString())??Array.Empty<string>());Message=status;
        if(solved?.Results.Issues.Count>0)AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,$"{solved.Results.Issues.Count} result quantities unavailable; inspect Unavailable.");
    }
}
