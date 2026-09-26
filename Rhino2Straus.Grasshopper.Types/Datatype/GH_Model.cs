using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2Straus.Core.Models;
namespace Rhino2Straus.Grasshopper.Datatype;
public class GH_Model : GH_Goo<ModelModel>, IGH_PreviewData
{
    private ModelPreview preview;
    public GH_Model(ModelModel model) { Value=model; }
    public GH_Model() { }
    private ModelPreview Preview => preview ??= new ModelPreview(Value);
    public override bool IsValid => Value != null;
    public override string TypeName => "Straus7 Model";
    public override string TypeDescription => "One assembled Straus7 model with geometry, properties, attributes, loads and optional immutable analysis results";
    public override IGH_Goo Duplicate() => Value == null ? new GH_Model() : new GH_Model(new ModelModel(Value));
    public override string ToString() => Value == null ? "Empty Straus7 model" : $"Straus7: {Value.NodeElements.Count} nodes, {Value.FrameElements.Count} beams, {Value.AreaElements.Count} plates, {Value.SolidElements.Count} solids, {Value.LinkElements.Count} links" + (Value.Results == null ? "" : $"; {Value.Results.Cases.Count} result cases");
    public BoundingBox ClippingBox => PreviewPolicy.AutomaticVisible()?Preview.Bounds:BoundingBox.Empty;
    public void DrawViewportWires(GH_PreviewWireArgs args) {if(PreviewPolicy.AutomaticVisible())Preview.DrawWires(args.Pipeline,args.Color);}
    public void DrawViewportMeshes(GH_PreviewMeshArgs args) {if(PreviewPolicy.AutomaticVisible())Preview.DrawMeshes(args.Pipeline,args.Material);}
}
