using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Display;
using Rhino.Geometry;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Settings;
using Rhino2Straus.Grasshopper.Datatype;

namespace Rhino2Straus.Grasshopper.Components.Extended;

public sealed class PreviewModelComponent : SafeComponent, IPreviewController
{
    private ModelPreview preview;
    private Color color;
    private bool menuEnabled=true, manageAutomatic=true;
    private PreviewLayer menuLayers=new PreviewSettings().Layers;
    public PreviewModelComponent():base("Rhino2Straus Display Manager","Rhino2Straus","Central preview controller: sections, loads and attributes. Right-click for visibility categories, About and Guide. Keeps the analysis Model unchanged.",ComponentTopics.Tab,ComponentTopics.Preview) { }
    public override Guid ComponentGuid=>new("5b506764-d527-4b1c-81da-910a202b9603");
    public override bool IsPreviewCapable=>true;
    public override bool IsBakeCapable=>Params.Output[4].VolatileDataCount>0;
    public override void BakeGeometry(Rhino.RhinoDoc doc,List<Guid> ids)=>BakeGeometry(doc,null,ids);
    public override void BakeGeometry(Rhino.RhinoDoc doc,Rhino.DocObjects.ObjectAttributes attributes,List<Guid> ids)
        =>PhysicalBrepOutput.Bake(this,doc,attributes,ids);
    public override BoundingBox ClippingBox=>preview?.Bounds??BoundingBox.Empty;
    public bool ManagesDocumentPreview=>manageAutomatic&&(Params.Input[0].SourceCount>0||Params.Input[0].VolatileDataCount>0);
    public bool AllowsResultPreview=>!Hidden&&!Locked&&preview?.Settings.Shows(PreviewLayer.Results)==true;
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model","M","One assembled Straus7 Model",GH_ParamAccess.item);
        p.AddBooleanParameter("Solid","S","Physical beam sections and plate thickness",GH_ParamAccess.item,true);
        p.AddBooleanParameter("Labels","L","Element and node identifiers",GH_ParamAccess.item,false);
        p.AddBooleanParameter("Axes","A","Local axes: 1 red, 2 green, 3 blue",GH_ParamAccess.item,false);
        p.AddBooleanParameter("Supports","R","Directional translation and rotation restraints",GH_ParamAccess.item,true);
        p.AddNumberParameter("Axis scale","Scale","Symbol size in model length units",GH_ParamAccess.item,1);
        p.AddColourParameter("Color","C","Geometry color",GH_ParamAccess.item,Color.FromArgb(50,150,190));
        p.AddBooleanParameter("Enabled","On","Master visibility switch, including manager-controlled automatic previews",GH_ParamAccess.item,true);
        p.AddGenericParameter("Settings","Settings","Optional Display Settings; overrides legacy Solid/Labels/Axes/Supports/scales inputs",GH_ParamAccess.item);p[8].Optional=true;
        p.AddTextParameter("Load cases","Cases","Exact case names; empty shows all input load cases. Combinations are not summed here.",GH_ParamAccess.list);p[9].Optional=true;
        p.AddNumberParameter("Load scale","Load scale","Visual multiplier; arrows capped at six symbol lengths. Numeric labels retain original values.",GH_ParamAccess.item,1);
        p.AddBooleanParameter("Manage previews","Manage","Suppress automatic preview of other Rhino2Straus components and FEM parameters in this document",GH_ParamAccess.item,true);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddTextParameter("Status","Status","Visible scene summary",GH_ParamAccess.item);
        p.AddTextParameter("Available cases","Cases","Names accepted by the case filter",GH_ParamAccess.list);
        p.AddGenericParameter("Settings","Settings","Effective display-only settings",GH_ParamAccess.item);
        p.AddTextParameter("Legend","Legend","Symbol and color guide",GH_ParamAccess.list);
        p.AddBrepParameter("Breps","Breps",PhysicalBrepOutput.Description,GH_ParamAccess.tree);
        ((IGH_PreviewObject)p[4]).Hidden=true; // The manager draws the scene; geometry remains available for casting/baking.
        p.AddTextParameter("Brep issues","Issues","Elements omitted from the closed Brep output and why",GH_ParamAccess.list);
    }
    protected override void BeforeSolveInstance(){preview=null;base.BeforeSolveInstance();}
    protected override void Solve(IGH_DataAccess da)
    {
        if(da.Iteration>0){preview=null;throw new ArgumentException("Connect one assembled Model; use Merge Straus Models before the Display Manager.");}
        var model=Item<GH_Model>(da,0);manageAutomatic=Item<bool>(da,11);color=Item<Color>(da,6);
        var layers=menuLayers;
        void Set(PreviewLayer flag,bool on){if(on)layers|=flag;else layers&=~flag;}
        Set(PreviewLayer.Sections,Item<bool>(da,1));Set(PreviewLayer.Labels,Item<bool>(da,2));Set(PreviewLayer.Axes,Item<bool>(da,3));Set(PreviewLayer.Supports,Item<bool>(da,4));
        PreviewSettings supplied=null;da.GetData(8,ref supplied);
        var settings=supplied??new PreviewSettings{Layers=layers,SymbolScale=Item<double>(da,5),LoadScale=Item<double>(da,10)};
        settings=settings with{Enabled=settings.Enabled&&Item<bool>(da,7)&&menuEnabled};
        var cases=new List<string>();da.GetDataList(9,cases);
        preview=new ModelPreview(model.Value,settings,cases);
        foreach(var warning in preview.Annotations.Warnings)AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,warning);
        Message=settings.Enabled?"Display ON":"Display OFF";
        da.SetData(0,$"Rhino2Straus: {(settings.Enabled?"ON":"OFF")}; {preview.Annotations.Marks.Count} symbols; automatic previews {(manageAutomatic?"managed":"independent")}");
        da.SetDataList(1,model.Value.LoadCases.Keys);da.SetData(2,settings);
        da.SetDataList(3,new[]{"Geometry: chosen color. Loads: orange. Attributes: magenta.","Axes: 1 red / 2 green / 3 blue. Curved arrows: right-hand moments.","Support triangles: fixed translation; circles at support: fixed rotation. Rings: beam releases.","Zigzags: springs; circles on stems: masses. Diamonds/text: temperature or preload.","Arrows show direction; scale is visual and capped. Labels retain applied values and units."});
        var breps=new ModelBrepGeometry(model.Value);
        da.SetDataTree(4,PhysicalBrepOutput.Tree(breps));da.SetDataList(5,breps.Issues);
        foreach(var issue in breps.Issues)AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,issue);
    }
    public override void DrawViewportWires(IGH_PreviewArgs args)
    {if(!Hidden&&!Locked&&preview!=null)preview.DrawWires(args.Display,Attributes.Selected?Color.LimeGreen:color);}
    public override void DrawViewportMeshes(IGH_PreviewArgs args)
    {
        if(Hidden||Locked||preview==null)return;
        using var material=new DisplayMaterial(Attributes.Selected?Color.LimeGreen:color,preview.Settings.Transparency);
        preview.DrawMeshes(args.Display,material);
    }
    protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
    {
        base.AppendAdditionalComponentMenuItems(menu);
        Menu_AppendSeparator(menu);
        Menu_AppendItem(menu,"Rhino2Straus — display enabled",(_,_)=>{RecordUndoEvent("Display visibility");menuEnabled=!menuEnabled;ExpireSolution(true);},true,menuEnabled);
        var categories=Menu_AppendItem(menu,"Visibility categories (unwired Settings)");
        categories.Enabled=Params.Input[8].SourceCount==0;
        foreach(var layer in Enum.GetValues<PreviewLayer>().Where(v=>v is not PreviewLayer.None and not PreviewLayer.All))
        {
            var captured=layer;
            bool visible=((preview?.Settings.Layers??menuLayers)&layer)!=0;
            Menu_AppendItem(categories.DropDown,layer.ToString(),(_,_)=>
            {
                RecordUndoEvent("Preview category");if(visible)menuLayers&=~captured;else menuLayers|=captured;
                int input=captured switch{PreviewLayer.Sections=>1,PreviewLayer.Labels=>2,PreviewLayer.Axes=>3,PreviewLayer.Supports=>4,_=>-1};
                if(input>=0&&Params.Input[input].SourceCount==0){var p=(Param_Boolean)Params.Input[input];p.PersistentData.Clear();p.PersistentData.Append(new GH_Boolean((menuLayers&captured)!=0));p.ExpireSolution(false);}
                ExpireSolution(true);
            },true,visible);
        }
        Menu_AppendItem(menu,"About Rhino2Straus",(_,_)=>MessageBox.Show("Rhino2Straus "+GetType().Assembly.GetName().Version+"\nRhino 8 / Grasshopper → Straus7 R3.\nOne unified Model for geometry, properties, loads, analysis and results.\n\nDisplay Manager controls geometry, sections, loads and attributes without changing the analysis model.\nPreview and icons do not require access to Straus7.","Rhino2Straus | About",MessageBoxButtons.OK,MessageBoxIcon.Information));
        Menu_AppendItem(menu,"Guide / helper",(_,_)=>MessageBox.Show("1. Connect Build Model or Merge Straus Models to Model.\n2. Use Enabled to turn the complete managed preview off.\n3. Connect Display Settings for per-category switches, shading, transparency and scales. A connected Settings input has priority over menu categories and legacy display inputs.\n4. Filter exact Load case names from Available cases; empty means all input cases, not combinations.\n5. Sections include offsets; generic sections need a supplied profile.\n6. Connect Breps to a Brep parameter, or use the component Bake menu to create closed Rhino solids. Brep issues lists excluded elements; bake works with display OFF.\n\nManage previews suppresses other Rhino2Straus previews in this document without changing their Hidden flags. Remove the manager or set Manage previews=false to restore them. Hiding the manager keeps its automatic previews suppressed.\nOther plugins and Rhino geometry are independent. Use the Results category to enable the preview of result diagrams and deformed geometry components across all five result panels.\n\nArrows are scaled symbols, not solver results. See the Legend output and docs/PREVIEW.md.","Rhino2Straus | Guide",MessageBoxButtons.OK,MessageBoxIcon.Information));
    }
    public override bool Write(GH_IWriter writer)
    {writer.SetBoolean("DisplayEnabled",menuEnabled);writer.SetInt32("DisplayLayers",(int)menuLayers);return base.Write(writer);}
    public override bool Read(GH_IReader reader)
    {if(reader.ItemExists("DisplayEnabled"))menuEnabled=reader.GetBoolean("DisplayEnabled");if(reader.ItemExists("DisplayLayers"))menuLayers=(PreviewLayer)reader.GetInt32("DisplayLayers")&PreviewLayer.All;return base.Read(reader);}
}
