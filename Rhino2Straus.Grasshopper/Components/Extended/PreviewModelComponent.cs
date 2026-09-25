using System;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Display;
using Rhino.Geometry;
using Rhino2Straus.Grasshopper.Datatype;

namespace Rhino2Straus.Grasshopper.Components.Extended;

public sealed class PreviewModelComponent : SafeComponent
{
    private ModelPreview preview;
    private bool solid, labels, axes, supports;
    private Color color;
    public PreviewModelComponent():base("Preview Straus Model","Preview","Preview assembled geometry with physical sections, thickness, offsets, IDs, properties, restraints and principal axes","Rhino2Straus","13-Preview") { }
    public override Guid ComponentGuid=>new("5b506764-d527-4b1c-81da-910a202b9603");
    public override bool IsPreviewCapable=>true;
    public override BoundingBox ClippingBox=>preview?.Bounds??BoundingBox.Empty;
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model","M","One assembled Straus7 model",GH_ParamAccess.item);
        p.AddBooleanParameter("Solid","S","Display physical cross sections and thickness",GH_ParamAccess.item,true);
        p.AddBooleanParameter("Labels","L","Node IDs, element IDs and property names",GH_ParamAccess.item,false);
        p.AddBooleanParameter("Axes","A","Principal/local axes: 1 red, 2 green, 3 blue",GH_ParamAccess.item,false);
        p.AddBooleanParameter("Supports","R","Mark restrained nodes",GH_ParamAccess.item,true);
        p.AddNumberParameter("Axis scale","Scale","Axis length in model length units",GH_ParamAccess.item,1);
        p.AddColourParameter("Color","C","Preview color",GH_ParamAccess.item,Color.FromArgb(50,150,190));
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) { p.AddTextParameter("Status","S","Preview summary",GH_ParamAccess.item); }
    protected override void BeforeSolveInstance(){preview=null;base.BeforeSolveInstance();}
    protected override void Solve(IGH_DataAccess da)
    {
        var model=Item<GH_Model>(da,0);solid=Item<bool>(da,1);labels=Item<bool>(da,2);axes=Item<bool>(da,3);supports=Item<bool>(da,4);
        double scale=Item<double>(da,5);if(!double.IsFinite(scale)||scale<=0)throw new ArgumentException("Axis scale must be positive.");color=Item<Color>(da,6);
        preview=new ModelPreview(model.Value,scale);da.SetData(0,model.ToString());
    }
    public override void DrawViewportWires(IGH_PreviewArgs args)
    { if(!Hidden&&!Locked&&preview!=null)preview.DrawWires(args.Display,Attributes.Selected?Color.LimeGreen:color,labels,axes,supports,solid); }
    public override void DrawViewportMeshes(IGH_PreviewArgs args)
    { if(!Hidden&&!Locked&&solid&&preview!=null)preview.DrawMeshes(args.Display,new DisplayMaterial(Attributes.Selected?Color.LimeGreen:color,.2)); }
}
