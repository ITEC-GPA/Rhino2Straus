using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2Straus.Core.Elements;

namespace Rhino2Straus.Grasshopper.Datatype;

public sealed class GH_SolidElement : GH_Goo<SolidElementModel>, IGH_PreviewData
{
    public GH_SolidElement() { }
    public GH_SolidElement(SolidElementModel value) { Value=new SolidElementModel(value); }
    public override bool IsValid => Value?.Material != null && Value.Nodes.Count is 4 or 6 or 8;
    public override string TypeName => "Straus Solid";
    public override string TypeDescription => "Straus7 linear solid element";
    public override string ToString() => Value==null?"Null Solid":$"Solid {Value.Id}: {Value.Nodes.Count} nodes, {Value.Material?.Name}";
    public override IGH_Goo Duplicate() => Value==null?new GH_SolidElement():new GH_SolidElement(Value);
    public BoundingBox ClippingBox => Value==null||!PreviewPolicy.AutomaticVisible()?BoundingBox.Empty:new BoundingBox(Value.Nodes.Select(n=>n.Position));
    public void DrawViewportWires(GH_PreviewWireArgs args) { if(IsValid&&PreviewPolicy.AutomaticVisible())args.Pipeline.DrawMeshWires(Value.ToMesh(),args.Color); }
    public void DrawViewportMeshes(GH_PreviewMeshArgs args) { if(IsValid&&PreviewPolicy.AutomaticVisible())args.Pipeline.DrawMeshShaded(Value.ToMesh(),args.Material); }
}
