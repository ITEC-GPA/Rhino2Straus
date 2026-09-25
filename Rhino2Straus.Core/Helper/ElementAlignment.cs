using System;
using Rhino.Geometry;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.Models;

namespace Rhino2Straus.Core.Helper;

public static class ElementAlignment
{
    private static double Rotation(Vector3d direction, Vector3d normal, Vector3d x, Vector3d y)
    {
        if (!direction.IsValid || !direction.Unitize() || !normal.Unitize()) throw new ArgumentException("Use a non-zero finite direction and a valid element.");
        var projected = direction - normal * (direction * normal);
        if (projected.Length < 1e-10 || !projected.Unitize()) throw new ArgumentException("Alignment direction is parallel to the beam axis or plate normal.");
        return Math.Atan2(projected * y, projected * x);
    }
    public static FrameElementModel Beam(FrameElementModel input, Vector3d direction, int axis = 1, bool principal = true)
    {
        if (axis != 1 && axis != 2) throw new ArgumentException("Choose transverse axis 1 or 2.");
        if (input.NodeStart.Position.DistanceTo(input.NodeEnd.Position) < 1e-10) throw new ArgumentException("Zero-length beam.");
        var beam = new FrameElementModel(input) { Angle = 0 };
        beam.GetLocalAxes(true, out var z, out var y, out var x, out _);
        beam.Angle = Rotation(direction, z, x, y) - (axis == 2 ? Math.PI / 2 : 0) + (principal ? 0 : beam.FrameSection.Angle);
        return beam;
    }
    public static AreaElementModel Plate(AreaElementModel input, Vector3d direction, int axis = 1)
    {
        if (axis != 1 && axis != 2) throw new ArgumentException("Choose in-plane axis 1 or 2.");
        var plate = new AreaElementModel(input) { Angle = 0 };
        plate.GetLocalAxes(out var x, out var y, out var z);
        plate.Angle = Rotation(direction, z, x, y) - (axis == 2 ? Math.PI / 2 : 0);
        return plate;
    }
    public static ModelModel Model(ModelModel input, Vector3d direction, bool beams, bool plates, int axis = 1)
    {
        var model = new ModelModel(input);
        if (beams) foreach (var beam in model.FrameElements.Values) beam.Angle = Beam(beam, direction, axis).Angle;
        if (plates) foreach (var plate in model.AreaElements.Values) plate.Angle = Plate(plate, direction, axis).Angle;
        model.ClearResults(); return model;
    }
}
