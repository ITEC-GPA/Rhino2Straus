using System;
using Grasshopper.Kernel;

namespace Rhino2Straus.Grasshopper;

public abstract class SafeComponent : GH_Component
{
    public override Rhino.Geometry.BoundingBox ClippingBox=>Datatype.PreviewPolicy.ComponentVisible(this)?base.ClippingBox:Rhino.Geometry.BoundingBox.Empty;
    public override void DrawViewportWires(IGH_PreviewArgs args)
    {
        if(Datatype.PreviewPolicy.ComponentVisible(this))base.DrawViewportWires(args);
    }
    public override void DrawViewportMeshes(IGH_PreviewArgs args)
    {
        if(Datatype.PreviewPolicy.ComponentVisible(this))base.DrawViewportMeshes(args);
    }
    protected override System.Drawing.Bitmap Icon => ComponentIcons.Get(Name, SubCategory);
    protected SafeComponent(string name, string nickname, string description, string category, string subcategory)
        : base(name, nickname, description, category, subcategory) { }

    protected sealed override void SolveInstance(IGH_DataAccess DA)
    {
        try { Message = null; Solve(DA); }
        catch (Exception error) { Message = "Error"; AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error.GetBaseException().Message); }
    }
    protected abstract void Solve(IGH_DataAccess DA);

    protected static T Item<T>(IGH_DataAccess da, int index)
    {
        T value = default;
        if (!da.GetData(index, ref value)) throw new ArgumentException($"Missing or invalid input {index + 1}.");
        return value;
    }
}
