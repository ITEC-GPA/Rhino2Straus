using System;
using System.Linq;
using Rhino.Geometry;
using Rhino2Straus.Core.Elements;

namespace Rhino2Straus.Core.Helper;

/// <summary>Rigid geometry placement. Loads and restraints expressed globally keep their world directions.</summary>
public static class ElementTransforms
{
    private static void Check(Transform t)
    { if(t.IsRigid(1e-9)!=TransformRigidType.Rigid)throw new NotSupportedException("FEM placement supports translation and rotation. Rebuild sections and attributes explicitly when scaling, mirroring or morphing."); }
    public static NodeElementModel Node(NodeElementModel value,Transform transform)
    { Check(transform);var p=value.Position;p.Transform(transform);var n=new NodeElementModel(value){X=p.X,Y=p.Y,Z=p.Z};return n; }
    private static double SignedAngle(Vector3d from,Vector3d to,Vector3d axis)=>Math.Atan2(Vector3d.Multiply(axis,Vector3d.CrossProduct(from,to)),Vector3d.Multiply(from,to));
    public static FrameElementModel Beam(FrameElementModel value,Transform transform)
    {
        Check(transform);value.GetLocalAxes(true,out _,out _,out var desired,out _);desired.Transform(transform);
        var b=new FrameElementModel(value){NodeStart=Node(value.NodeStart,transform),NodeEnd=Node(value.NodeEnd,transform),Angle=0};
        b.GetLocalAxes(true,out var z,out _,out var baseline,out _);b.Angle=SignedAngle(baseline,desired,z);return b;
    }
    public static AreaElementModel Plate(AreaElementModel value,Transform transform)
    {
        Check(transform);value.GetLocalAxes(out var desired,out _,out _);desired.Transform(transform);
        var p=new AreaElementModel(value){NodeList=value.NodeList.Select(n=>Node(n,transform)).ToList(),Angle=0};
        p.GetLocalAxes(out var baseline,out _,out var z);p.Angle=SignedAngle(baseline,desired,z);return p;
    }
    public static LinkElementModel Link(LinkElementModel value,Transform transform)
    { Check(transform);return new LinkElementModel(value){NodeStart=Node(value.NodeStart,transform),NodeEnd=Node(value.NodeEnd,transform)}; }
}
