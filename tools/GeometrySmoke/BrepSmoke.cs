using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Models;
using Rhino2Straus.Grasshopper.Datatype;

internal static class BrepSmoke
{
    internal static void Geometry(Action<bool,string> check,AreaElementModel plate,ModelModel model)
    {
        var converted=new ModelBrepGeometry(model);
        check(converted.Issues.Count==0&&converted.Elements.Count==3&&converted.Elements.All(e=>e.Geometry.IsValid&&e.Geometry.IsSolid),"Model Breps include closed beam, plate and solid volumes only");
        check(converted.Elements.Select(e=>e.Kind).SequenceEqual(new[]{0,1,2})&&converted.Elements.All(e=>e.Geometry.GetUserString("Rhino2Straus.ElementId")==e.ElementId.ToString()),"Brep geometry retains element kinds and Model IDs");
        using var plateBrep=ModelBrepGeometry.Plate(plate);
        using var properties=VolumeMassProperties.Compute(plateBrep);
        check(Math.Abs(properties.Volume-.4)<1e-9&&Math.Abs(properties.Centroid.Z-.7)<1e-9,"Closed plate Brep retains physical thickness and combined offsets");
        var farPlate=ElementTransforms.Plate(plate,Transform.Translation(1000000,1000000,1000000));
        using var farBrep=ModelBrepGeometry.Plate(farPlate);
        using var farVolume=VolumeMassProperties.Compute(farBrep);
        check(Math.Abs(farVolume.Volume-.4)<1e-6,"Brep conversion preserves thin geometry at large coordinates");
        var unitSolids=new[]{
            new[]{new Point3d(0,0,0),new(1,0,0),new(0,1,0),new(0,0,1)},
            new[]{new Point3d(0,0,0),new(1,0,0),new(0,1,0),new(0,0,1),new(1,0,1),new(0,1,1)},
            new[]{new Point3d(0,0,0),new(1,0,0),new(1,1,0),new(0,1,0),new(0,0,1),new(1,0,1),new(1,1,1),new(0,1,1)}};
        foreach(var points in unitSolids)
        {
            var solid=new SolidElementModel{Material=MaterialModel.S355,Nodes=points.Select(p=>new NodeElementModel(p)).ToList()};
            using var brep=ModelBrepGeometry.Solid(solid);using var volume=VolumeMassProperties.Compute(brep);
            double expected=points.Length==4?1d/6:points.Length==6?.5:1;
            check(brep.IsSolid&&brep.SolidOrientation==BrepSolidOrientation.Outward&&Math.Abs(volume.Volume-expected)<1e-9,$"{points.Length}-node solid converts to a closed outward Brep with correct volume");
        }
        foreach(var shape in new[]{FrameSectionModel.SectionGeometryTypes.P,FrameSectionModel.SectionGeometryTypes.B})
        {
            var profile=new FrameSectionModel("Hollow",shape,FrameSectionModel.Types.DBUSER,FrameSectionModel.OffsetTypes.CC,.4,shape==FrameSectionModel.SectionGeometryTypes.P?.02:.2,.02,.03,.2,.03);
            var beam=new FrameElementModel(new(0,0,0),new(2,0,0),new FramePropertyModel("Hollow",MaterialModel.S355,profile),0,Vector3d.Zero);
            var breps=ModelBrepGeometry.Beam(beam);using var volume=VolumeMassProperties.Compute(breps.Single());
            check(Math.Abs(volume.Volume-2*profile.SectionArea)<1e-8,$"{shape} Brep preserves the section void instead of filling it");
        }
        var disconnected=new ModelModel(model);var disconnectedSection=disconnected.FrameElements.Values.Single().FrameSection;
        var extra=disconnectedSection.Breps[0].DuplicateBrep();extra.Translate(0,2,0);
        disconnectedSection.Breps[0].Append(extra);
        var parts=new ModelBrepGeometry(disconnected);
        check(parts.Issues.Count==0&&parts.Elements.Count(e=>e.Kind==0)==2,"Multipart sections keep every disconnected face as a separate solid");
        var missing=new ModelModel(model);missing.FrameElements.Values.Single().FrameSection.Breps.Clear();
        missing.AreaElements.Values.Single().AreaThickness.ThicknessMembrane=0;
        var incomplete=new ModelBrepGeometry(missing);
        check(incomplete.Elements.Count==1&&incomplete.Elements[0].Kind==2&&incomplete.Issues.Count==2,"Missing section profiles and zero plate thickness produce explicit issues, not open solids");
        var hash=ModelFingerprint.Compute(model);converted.Elements[0].Geometry.Translate(100,200,300);
        check(ModelFingerprint.Compute(model)==hash&&new ModelBrepGeometry(model).Elements[0].Geometry.GetBoundingBox(true).Center.DistanceTo(converted.Elements[0].Geometry.GetBoundingBox(true).Center)>100,"Downstream Brep edits leave the FEM model and new conversions unchanged");
    }

    internal static void Grasshopper(Action<bool,string> check,GH_Component manager,ModelModel model)
    {
        var beam=new GH_FrameElement(model.FrameElements.Values.Single());beam.Value.BuildBreps();beam.Value.Offset+=new Vector3d(.3,.1,0);
        GH_Brep beamBrep=null,plateBrep=null,multiBrep=null;
        check(((IGH_Goo)beam).CastTo(out beamBrep)&&beamBrep.Value.IsSolid&&beamBrep.Value.GetBoundingBox(true).Center.DistanceTo(ModelBrepGeometry.Beam(beam.Value).Single().GetBoundingBox(true).Center)<1e-9,"Beam cast uses current offsets instead of a stale preview cache");
        var plate=new GH_AreaElement(model.AreaElements.Values.Single());
        check(((IGH_Goo)plate).CastTo(out plateBrep)&&plateBrep.Value.IsSolid,"Plate casts to a closed GH_Brep");
        var solid=new GH_SolidElement(model.SolidElements.Values.Single());GH_Brep solidBrep=null;
        check(solid.CastTo(ref solidBrep)&&solidBrep.Value.IsSolid,"Solid casts to a closed GH_Brep");
        Brep rawSolid=null,rawBeam=null,rawPlate=null;
        check(((IGH_Goo)beam).CastTo(out rawBeam)&&((IGH_Goo)plate).CastTo(out rawPlate)&&solid.CastTo(ref rawSolid)&&rawBeam.IsSolid&&rawPlate.IsSolid&&rawSolid.IsSolid,"Element casts also support native Rhino Brep values");
        var multi=new GH_FrameElement(beam.Value);multi.Value.FrameSection=new FrameSectionModel(beam.Value.FrameSection);
        multi.Value.FrameSection.Breps.Add(multi.Value.FrameSection.Breps[0].DuplicateBrep());
        check(!((IGH_Goo)multi).CastTo(out multiBrep),"Single-object cast refuses to silently discard multipart section volumes");
        using(var castDocument=new GH_Document())
        {
            var source=new Param_GenericObject();castDocument.AddObject(source,false);
            source.PersistentData.Append(beam);source.PersistentData.Append(plate);source.PersistentData.Append(solid);
            source.CollectData();source.ComputeData();
            var target=new Param_Brep();castDocument.AddObject(target,false);target.AddSource(source);target.CollectData();target.ComputeData();
            check(target.VolatileDataCount==3&&target.VolatileData.AllData(true).OfType<GH_Brep>().All(b=>b.Value.IsSolid),"Actual Brep parameter connection casts all three element types");
        }
        var output=manager.Params.Output[4];
        check(output.VolatileDataCount==3&&output.VolatileData.Paths.Select(p=>p.ToString()).SequenceEqual(new[]{"{0;1}","{1;1}","{2;1}"}),"Preview Breps retain type and ID branches while the display is OFF");
        check(((IGH_PreviewObject)output).Hidden,"Brep output does not duplicate the manager preview");
        Bake(check,manager);
    }

    internal static void Bake(Action<bool,string> check,GH_Component component)
    {
        using var doc=RhinoDoc.CreateHeadless(null);
        var ids=new List<Guid>();var attributes=new ObjectAttributes{Name="Physical solid",ObjectColor=Color.CadetBlue,ColorSource=ObjectColorSource.ColorFromObject};
        var baker=(IGH_BakeAwareObject)component;
        check(baker.IsBakeCapable,component.Name+" exposes native Grasshopper Bake");
        baker.BakeGeometry(doc,attributes,ids);
        check(ids.Count==3&&ids.All(id=>doc.Objects.FindId(id)?.Geometry is Brep b&&b.IsValid&&b.IsSolid),"Native Grasshopper Bake creates only three closed Rhino Breps, without meshes or annotations");
        check(ids.All(id=>doc.Objects.FindId(id).Attributes.Name==attributes.Name&&doc.Objects.FindId(id).Geometry.GetUserString("Rhino2Straus.ElementId")=="1"),"Bake preserves chosen Rhino attributes and element identity");
    }
}
