using System;
using System.IO;
using System.Linq;
using Grasshopper.Kernel;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Grasshopper.Datatype;
using Rhino2Straus.Import;

namespace Rhino2Straus.Grasshopper.Components.Extended;

public sealed class ReadStrausModelComponent : SafeComponent
{
    private readonly TriggeredOperation<ModelImportResult> operation=new();
    public ReadStrausModelComponent() : base("Read Straus Model", "Read ST7 Model", "Import an existing .st7 into the unified Grasshopper Model. Reads an isolated copy, normalises to m/kN/kPa/tonne/C and preserves distinct coincident nodes. See Report for unsupported attributes. Requires the Straus7 R3 API licence.", ComponentTopics.Tab, ComponentTopics.Files) { }
    public override Guid ComponentGuid => new Guid("bd763938-69ba-4f07-87e7-b61948629344");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Model path","Path","Existing .st7 file; the source is never saved or overwritten",GH_ParamAccess.item);
        p.AddBooleanParameter("Read","Read","False-to-true transition. Repeat to refresh the file",GH_ParamAccess.item,false);
        p.AddIntegerParameter("Freedom case","FC","Restraint case to import",GH_ParamAccess.item,1);
        p.AddBooleanParameter("Allow partial","Partial","False withholds Model when unsupported structural data is found. True explicitly accepts omissions listed in Report",GH_ParamAccess.item,false);
        p.AddNumberParameter("Tolerance","Tol","Geometric validation tolerance in metres; distinct native nodes remain distinct",GH_ParamAccess.item,.001);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Model","Model","Unified Model for preview, decompose, edit, export and analysis",GH_ParamAccess.item);
        p.AddBooleanParameter("Complete","Complete","No omissions detected within the supported elastic structural scope; inspect Report",GH_ParamAccess.item);
        p.AddTextParameter("Report","Report","Import scope, unsupported data and preflight diagnostics",GH_ParamAccess.list);
        p.AddTextParameter("Identity map","IDs","Entity | native number | native label | Model ID",GH_ParamAccess.list);
        p.AddTextParameter("Status","Status","Status of the last read",GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {
        string path=Path.GetFullPath(Item<string>(da,0));int freedom=Item<int>(da,2);bool partial=Item<bool>(da,3);double tolerance=Item<double>(da,4);
        var file=new FileInfo(path);
        string key=System.Text.Json.JsonSerializer.Serialize(new object[]{path,file.Exists?file.Length:0,file.Exists?file.LastWriteTimeUtc.Ticks:0,freedom,partial,tolerance});
        var result=operation.Update(key,Item<bool>(da,1),()=>StrausModelImport.Read(path,freedom,partial,tolerance),
            r=>r.Complete?"Imported":r.Model!=null?"Partial import":"Incomplete - inspect Report", "Read required");
        da.DisableGapLogic(0);
        if(result?.Model!=null)da.SetData(0,new GH_Model(result.Model));
        da.SetData(1,result?.Complete??false);
        da.SetDataList(2,result?.Issues.Select(i=>(i.DataOmitted?"OMITTED | ":"INFO | ")+i)??Array.Empty<string>());
        da.SetDataList(3,result?.Identities.Select(i=>$"{i.Entity} | {i.NativeNumber} | {i.NativeLabel} | {i.ModelId}")??Array.Empty<string>());
        da.SetData(4,operation.Status);Message=operation.Status;
        if(result?.Complete==false)AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,"Unsupported data found. Inspect Report before using a partial Model.");
    }
}
