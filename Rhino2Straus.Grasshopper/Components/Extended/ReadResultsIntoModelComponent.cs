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

public sealed class ReadResultsIntoModelComponent : SafeComponent
{
    private bool wasRun;private ModelModel solved;private string cachedKey="",status="Idle";
    public ReadResultsIntoModelComponent() : base("Read Results Into Model", "Read Results Into Model", "Read Results Into Model", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("ea7fe876-9b0c-5339-a2e4-4bb6c4acfad3");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        p.AddTextParameter("Model path", "Model path", "Model path", GH_ParamAccess.item);
        p.AddTextParameter("Result path", "Result path", "Result path", GH_ParamAccess.item);
        p.AddBooleanParameter("Read", "Read", "Button or false-to-true transition", GH_ParamAccess.item, false);
        p.AddIntegerParameter("Stations", "Stations", "Stations", GH_ParamAccess.item, 11);
        p.AddIntegerParameter("Sample", "Sample", "Sample", GH_ParamAccess.item, 1);
        p.AddBooleanParameter("Allow unverified", "Allow unverified", "Explicit opt-in for externally solved files. The .st7 must still carry this Model identity. Verifiers should require provenance.", GH_ParamAccess.item, false);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        p.AddTextParameter("Status", "Status", "Status", GH_ParamAccess.item);
        p.AddTextParameter("Unavailable", "Unavailable", "Unavailable", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var input=Item<GH_Model>(da,0).Value;bool run=Item<bool>(da,3);bool fire=run&&!wasRun;wasRun=run;
        string key=ModelFingerprint.Compute(input)+System.Text.Json.JsonSerializer.Serialize(new object[]{Item<string>(da,1),Item<string>(da,2),Item<int>(da,4),Item<int>(da,5),Item<bool>(da,6)});
        if(key!=cachedKey){solved=null;status="Read required";}
        if(fire){solved=null;status="Failed";cachedKey=key;solved=ModelAnalysisService.Read(input,Item<string>(da,1),Item<string>(da,2),Item<int>(da,4),Item<int>(da,5),Item<bool>(da,6));status=solved.Results.ProvenanceVerified?"Read verified results":"Read unverified results";}
        da.SetData(0,new GH_Model(solved??input));da.SetData(1,status);da.SetDataList(2,solved?.Results.Issues.Select(x=>x.ToString())??Array.Empty<string>());Message=status;
        if(solved?.Results.ProvenanceVerified==false)AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,"The native input/result file pairing has no verified manifest.");
    }
}
