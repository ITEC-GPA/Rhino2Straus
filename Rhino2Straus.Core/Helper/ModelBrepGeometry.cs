using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.Models;

namespace Rhino2Straus.Core.Helper;

/// <summary>One closed physical volume. Kind: 0 beam, 1 plate, 2 solid; IDs are Model IDs.</summary>
public sealed record ElementBrep(int Kind, int ElementId, int PartIndex, Brep Geometry);

/// <summary>Display-independent geometry for downstream Brep operations and Rhino bake. No boolean union or native API.</summary>
public sealed class ModelBrepGeometry
{
    public List<ElementBrep> Elements { get; } = new();
    public List<string> Issues { get; } = new();

    public ModelBrepGeometry(ModelModel model)
    {
        if(model==null)throw new ArgumentNullException(nameof(model));
        foreach(var b in model.FrameElements.Values.OrderBy(e=>e.Id))Add(0,b.Id,()=>Beam(b));
        foreach(var p in model.AreaElements.Values.OrderBy(e=>e.Id))Add(1,p.Id,()=>new[]{Plate(p)});
        foreach(var s in model.SolidElements.Values.OrderBy(e=>e.Id))Add(2,s.Id,()=>new[]{Solid(s)});
    }

    private void Add(int kind,int id,Func<IEnumerable<Brep>> build)
    {
        try
        {
            var parts=build().ToArray();
            if(parts.Length==0)throw new InvalidOperationException("No section profile. Supply a planar profile for a generic section.");
            for(int i=0;i<parts.Length;i++)
            {
                var geometry=parts[i];
                geometry.SetUserString("Rhino2Straus.ElementType",new[]{"Beam","Plate","Solid"}[kind]);
                geometry.SetUserString("Rhino2Straus.ElementId",id.ToString(System.Globalization.CultureInfo.InvariantCulture));
                geometry.SetUserString("Rhino2Straus.PartIndex",i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                Elements.Add(new(kind,id,i,geometry));
            }
        }
        catch(Exception error) when(error is ArgumentException or InvalidOperationException)
        {Issues.Add($"{new[]{"Beam","Plate","Solid"}[kind]} {id}: {error.Message}");}
    }

    public static List<Brep> Beam(FrameElementModel beam)
    {
        if(beam==null)throw new ArgumentNullException(nameof(beam));
        var breps=PreviewGeometry.Beam(beam);
        if(breps.Count!=(beam.FrameSection?.Breps?.Sum(b=>b.Faces.Count)??0))
            throw new InvalidOperationException("One or more section faces could not be extruded.");
        return breps.Select(RequireSolid).ToList();
    }
    public static Brep Plate(AreaElementModel plate)
    {
        if(plate?.AreaThickness==null||plate.AreaThickness.ThicknessMembrane<=0)
            throw new InvalidOperationException("A positive membrane thickness is required for a closed plate Brep.");
        using var mesh=PreviewGeometry.Plate(plate);
        return FromMesh(mesh);
    }
    public static Brep Solid(SolidElementModel solid)
    {
        if(solid==null)throw new ArgumentNullException(nameof(solid));
        using var mesh=solid.ToMesh();
        return FromMesh(mesh);
    }
    private static Brep FromMesh(Mesh mesh)
    {
        if(!mesh.IsValid||!mesh.IsClosed)throw new InvalidOperationException("The element skin is not a valid closed mesh.");
        return RequireSolid(Brep.CreateFromMesh(mesh,true));
    }
    private static Brep RequireSolid(Brep brep)
    {
        if(brep==null||!brep.IsValid||!brep.IsSolid)throw new InvalidOperationException("Could not create a valid closed Brep.");
        if(brep.SolidOrientation==BrepSolidOrientation.Inward)brep.Flip();
        using var volume=VolumeMassProperties.Compute(brep);
        if(volume==null||!double.IsFinite(volume.Volume)||volume.Volume<=0)
            throw new InvalidOperationException("The Brep has no positive finite volume.");
        return brep;
    }
}
