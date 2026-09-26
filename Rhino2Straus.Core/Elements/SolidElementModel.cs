using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using Rhino2Straus.Core.Base;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.ElementProperties;

namespace Rhino2Straus.Core.Elements;

public sealed record SolidPressure(LoadCaseModel LoadCase, int Face, double Value);

/// <summary>Linear Tet4, Wedge6 or Hexa8; nodes follow the Straus7 local connectivity convention.</summary>
public sealed class SolidElementModel : ElementModel
{
    public List<NodeElementModel> Nodes { get; set; } = new();
    public MaterialModel Material { get; set; }
    public List<SolidPressure> Pressures { get; set; } = new();
    public SolidElementModel() { }
    public SolidElementModel(SolidElementModel source) : base(source)
    { Nodes=new(source.Nodes); Material=source.Material; Pressures=new(source.Pressures); }
    public static int[][] Faces(int count) => count switch
    {
        4 => new[]{new[]{0,2,1},new[]{0,1,3},new[]{1,2,3},new[]{2,0,3}},
        6 => new[]{new[]{0,2,1},new[]{3,4,5},new[]{0,1,4,3},new[]{1,2,5,4},new[]{2,0,3,5}},
        8 => new[]{new[]{0,3,2,1},new[]{4,5,6,7},new[]{0,1,5,4},new[]{1,2,6,5},new[]{2,3,7,6},new[]{3,0,4,7}},
        _ => throw new ArgumentException("Use 4, 6 or 8 nodes in Straus7 order.")
    };
    // Face triangulation is for geometric preview only; load face IDs use the native API convention.
    public Mesh ToMesh()
    {
        var mesh=new Mesh();mesh.Vertices.UseDoublePrecisionVertices=true;foreach(var n in Nodes)mesh.Vertices.Add(n.Position);
        foreach(var f in Faces(Nodes.Count))if(f.Length==3)mesh.Faces.AddFace(f[0],f[1],f[2]);else mesh.Faces.AddFace(f[0],f[1],f[2],f[3]);
        mesh.Normals.ComputeNormals();mesh.Compact();return mesh;
    }
    public double SignedVolume()
    {
        double volume=0;var origin=Nodes[0].Position;
        foreach(var f in Faces(Nodes.Count))for(int j=1;j<f.Length-1;j++)
            volume+=Vector3d.Multiply(Nodes[f[0]].Position-origin,Vector3d.CrossProduct(Nodes[f[j]].Position-origin,Nodes[f[j+1]].Position-origin))/6;
        return volume;
    }
}
