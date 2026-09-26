using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using Rhino2Straus.Core.Elements;

namespace Rhino2Straus.Core.Helper;

/// <summary>Physical geometry uses the same principal offsets exported to Straus7.</summary>
public static class PreviewGeometry
{
    public static List<Brep> Beam(FrameElementModel beam)
    {
        var result = new List<Brep>();
        if (beam.FrameSection?.Breps == null || beam.NodeStart == null || beam.NodeEnd == null) return result;
        beam.GetLocalAxes(false, out var longitudinal, out var localY, out var localX, out var transform);
        beam.GetLocalAxes(true, out _, out var principal2, out var principal1, out _);
        var offset = beam.GetEffectiveOffset();
        var shift = principal1 * offset.X + principal2 * offset.Y;
        foreach (var profile in beam.FrameSection.Breps)
        {
            using var face = profile.DuplicateBrep();
            face.Transform(transform); face.Translate(shift);
            using var path = new LineCurve(beam.NodeStart.Position + shift, beam.NodeEnd.Position + shift);
            foreach(var sectionFace in face.Faces)
            {
                // Rhino's cap operation must receive an isolated face for multipart profiles.
                using var singleFace=sectionFace.DuplicateFace(false);
                var solid = singleFace.Faces[0].CreateExtrusion(path, true);
                if (solid != null)
                {
                    if(solid.SolidOrientation==BrepSolidOrientation.Inward)solid.Flip();
                    result.Add(solid);
                }
            }
        }
        return result;
    }

    public static Mesh Plate(AreaElementModel plate, bool thickness = true)
    {
        var mesh = new Mesh();
        mesh.Vertices.UseDoublePrecisionVertices=true;
        if (plate.NodeList == null || (plate.NodeList.Count != 3 && plate.NodeList.Count != 4)) return mesh;
        plate.GetLocalAxes(out _, out _, out var normal);
        double offset = plate.Offset + (plate.AreaThickness?.Offset ?? 0);
        double t = thickness ? plate.AreaThickness?.ThicknessMembrane ?? 0 : 0;
        int n = plate.NodeList.Count;
        foreach (var node in plate.NodeList) mesh.Vertices.Add(node.Position + normal * (offset - t / 2));
        if (t <= 0)
        {
            if (n == 3) mesh.Faces.AddFace(0, 1, 2); else mesh.Faces.AddFace(0, 1, 2, 3);
        }
        else
        {
            foreach (var node in plate.NodeList) mesh.Vertices.Add(node.Position + normal * (offset + t / 2));
            if (n == 3) { mesh.Faces.AddFace(2, 1, 0); mesh.Faces.AddFace(3, 4, 5); }
            else { mesh.Faces.AddFace(3, 2, 1, 0); mesh.Faces.AddFace(4, 5, 6, 7); }
            for (int i = 0; i < n; i++) { int j = (i + 1) % n; mesh.Faces.AddFace(i, j, j + n, i + n); }
        }
        mesh.Normals.ComputeNormals(); mesh.Compact(); return mesh;
    }

    public static BoundingBox BeamBounds(FrameElementModel beam)
    {
        var bounds = new BoundingBox(new[] { beam.NodeStart.Position, beam.NodeEnd.Position });
        foreach (var brep in Beam(beam)) bounds.Union(brep.GetBoundingBox(true));
        return bounds;
    }
}
