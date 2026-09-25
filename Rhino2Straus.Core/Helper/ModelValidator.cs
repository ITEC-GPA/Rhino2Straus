using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Loads;
using Rhino2Straus.Core.Models;

namespace Rhino2Straus.Core.Helper;

public sealed class ModelValidation
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    public bool IsValid => Errors.Count == 0;
}

/// <summary>Preflight checks; this is not a structural adequacy or stability check.</summary>
public static class ModelValidator
{
    public static ModelValidation Validate(ModelModel model)
    {
        var v = new ModelValidation();
        void Error(bool condition, string message) { if (condition) v.Errors.Add(message); }
        bool Bad(double x) => !double.IsFinite(x);
        bool BadVector(Vector3d x) => Bad(x.X) || Bad(x.Y) || Bad(x.Z);
        bool Negative(Vector3d x) => BadVector(x) || x.X < 0 || x.Y < 0 || x.Z < 0;
        if (model == null) { v.Errors.Add("Model is missing."); return v; }
        if (model.ModelUnits == null) { v.Errors.Add("Model units are missing."); return v; }
        try { StrausUnits.ToApi(model.ModelUnits); } catch (Exception e) { v.Errors.Add(e.Message); }
        double tol = model.ModelUnits.Tolerance;
        Error(Bad(tol) || tol <= 0, "Node merge tolerance must be finite and positive.");
        Error(model.NodeElements.Count == 0, "Model has no nodes.");
        foreach (var node in model.NodeElements.Values)
        {
            Error(node.Id < 1 || !node.Position.IsValid, $"Node {node.Id}: invalid identifier or coordinates.");
            Error(Negative(node.TranslationSpring) || Negative(node.RotationSpring) || Negative(node.TranslationMass) || Negative(node.RotationMass), $"Node {node.Id}: stiffnesses and masses must be finite and nonnegative.");
            foreach (var load in node.NodalLoadList)
                Error(load.LoadCase == null || new[] { load.FX, load.FY, load.FZ, load.MX, load.MY, load.MZ }.Any(Bad), $"Node {node.Id}: invalid nodal load.");
            Error(node.PrescribedDisplacement?.Length != 6 || node.PrescribedDisplacement.Any(Bad), $"Node {node.Id}: six finite imposed displacement values are required.");
            foreach (var load in node.Temperatures) Error(load.LoadCase == null || Bad(load.Temperature), $"Node {node.Id}: invalid temperature.");
            Error(node.Temperatures.GroupBy(x => x.LoadCase?.Name).Any(x => x.Count() > 1), $"Node {node.Id}: more than one temperature in the same load case.");
        }
        foreach (var mat in model.Materials.Values)
        {
            Error(Bad(mat.ModulusElasticity) || mat.ModulusElasticity <= 0, $"Material {mat.Name}: E must be positive (kPa).");
            Error(Bad(mat.PoissonRatio) || mat.PoissonRatio <= -1 || mat.PoissonRatio >= 0.5, $"Material {mat.Name}: Poisson ratio must be between -1 and 0.5.");
            Error(Bad(mat.Density) || mat.Density < 0, $"Material {mat.Name}: density must be nonnegative (tonne/m³).");
        }
        foreach(var mat in model.Materials.Values)Error(new[]{mat.ThermalCoefficient,mat.DampingRatio,mat.ViscousDamping,mat.Conductivity,mat.SpecificHeat}.Any(Bad),$"Material {mat.Name}: nonfinite thermal or damping data.");
        foreach (var duplicate in model.FrameProperties.Values.GroupBy(x => x.Name).Where(x => x.Count() > 1)) v.Errors.Add($"Duplicate beam property name: {duplicate.Key}.");
        foreach (var duplicate in model.AreaProperties.Values.GroupBy(x => x.Name).Where(x => x.Count() > 1)) v.Errors.Add($"Duplicate plate property name: {duplicate.Key}.");
        foreach (var beam in model.FrameElements.Values)
        {
            Error(beam.NodeStart == null || beam.NodeEnd == null || beam.NodeStart.Position.DistanceTo(beam.NodeEnd.Position) <= tol, $"Beam {beam.Id}: zero length or missing nodes.");
            Error(beam.FrameProperty == null || beam.FrameSection == null || beam.Material == null, $"Beam {beam.Id}: missing section or material.");
            Error(Bad(beam.Angle) || BadVector(beam.Offset) || beam.Offset.Z != 0, $"Beam {beam.Id}: invalid angle or offset.");
            if(beam.FrameProperty!=null)Error(!Enum.IsDefined(beam.FrameProperty.Behaviour),$"Beam {beam.Id}: invalid behaviour.");
            if(beam.FrameSection?.Type==FrameSectionModel.Types.COMPOSITE_I)v.Errors.Add($"Beam {beam.Id}: composite section stiffness is not implemented; use calibrated generic properties.");
            if (beam.FrameSection != null) Error(beam.FrameSection.SectionArea <= 0 || Bad(beam.FrameSection.SectionArea), $"Beam {beam.Id}: section area must be positive.");
            if(beam.FrameSection is { } section)
            {
                Error(section.SectionGeometryType==FrameSectionModel.SectionGeometryTypes.B&&section.Dimension4!=section.Dimension5,$"Beam {beam.Id}: Straus7 standard RHS requires equal top/bottom thickness. Use a BXS or calibrated generic section for asymmetric RHS.");
                Error(section.SectionGeometryType==FrameSectionModel.SectionGeometryTypes.C&&(section.Dimension2!=section.Dimension5||section.Dimension4!=section.Dimension6),$"Beam {beam.Id}: Straus7 standard channel requires equal flanges. Use a BXS or calibrated generic section for asymmetric channels.");
                Error(section.SectionGeometryType==FrameSectionModel.SectionGeometryTypes.H&&section.Dimension7!=0,$"Beam {beam.Id}: fillet radius requires a BXS section; standard API I-sections use sharp corners.");
                Error(section.BxsData!=null&&section.BxsLengthUnit!=model.ModelUnits.LengthUnit,$"Beam {beam.Id}: BXS was read with different length units; reread it using this model units.");
                if(section.SectionGeometryType==FrameSectionModel.SectionGeometryTypes.Generic)Error(new[]{section.I11,section.I22,section.J,section.ShearA1,section.ShearA2}.Any(x=>Bad(x)||x<0),$"Beam {beam.Id}: invalid generic section stiffness.");
            }
            Error(beam.StartReleases?.Length != 6 || beam.EndReleases?.Length != 6, $"Beam {beam.Id}: releases need six flags per end.");
            foreach (var load in beam.FrameLoadList)
            {
                Error(load.LoadCase == null, $"Beam {beam.Id}: missing load case.");
                if (load is FrameLoadStrausModel d)
                {
                    Error(!Enum.IsDefined(d.LoadSchema) || d.LoadSchema == FrameLoadStrausModel.LoadSchemas.Undefined || !Enum.IsDefined(d.Direction) || !Enum.IsDefined(d.LoadType), $"Beam {beam.Id}: invalid distributed load option.");
                    Error(new[] { d.A, d.B, d.PA, d.PB, d.P1, d.P2 }.Any(Bad) || d.A < 0 || d.B > 1 || d.B <= d.A, $"Beam {beam.Id}: distributed load positions must satisfy 0 ≤ A < B ≤ 1.");
                    Error(d.LoadType == FrameLoadStrausModel.FrameLoadTypes.Moment && d.CoordinateSystem != CoordinateSystemModel.Local, $"Beam {beam.Id}: distributed moments require principal axes in the Straus7 API.");
                    Error(d.CoordinateSystem == CoordinateSystemModel.Local && ((int)d.Direction > 2 || d.IsProjected), $"Beam {beam.Id}: projection and gravity directions require global axes.");
                }
                else if (load is FramePointLoadModel p) Error(BadVector(p.Value) || Bad(p.Position) || p.Position < 0 || p.Position > 1, $"Beam {beam.Id}: invalid point load or relative position.");
                else if (load is FrameTemperatureGradientModel t) Error(Bad(t.Gradient1) || Bad(t.Gradient2), $"Beam {beam.Id}: invalid temperature gradient.");
                else if (load is FramePreloadModel p2) Error(Bad(p2.Value), $"Beam {beam.Id}: invalid preload.");
                else v.Errors.Add($"Beam {beam.Id}: unsupported load {load.GetType().Name}.");
            }
            Error(beam.FrameLoadList.Where(x => x is FrameTemperatureGradientModel || x is FramePreloadModel).GroupBy(x => (x.GetType(), x.LoadCase?.Name)).Any(g => g.Count() > 1), $"Beam {beam.Id}: duplicate gradient or preload in one case; combine values first.");
        }
        foreach (var plate in model.AreaElements.Values)
        {
            Error(plate.NodeList == null || (plate.NodeList.Count != 3 && plate.NodeList.Count != 4), $"Plate {plate.Id}: only Tri3 and Quad4 are supported.");
            Error(plate.AreaProperty == null || plate.AreaThickness == null || plate.Material == null, $"Plate {plate.Id}: missing property or material.");
            Error(plate.AreaProperty!=null&&(!Enum.IsDefined(plate.AreaProperty.PropertyType)||plate.AreaProperty.PropertyType==AreaPropertyModel.PlatePropertyType.Undefined),$"Plate {plate.Id}: unsupported property type.");
            Error(Bad(plate.Angle)||Bad(plate.Offset)||Bad(plate.AreaThickness?.Offset??0),$"Plate {plate.Id}: invalid angle or offset.");
            if (plate.AreaThickness != null) Error(plate.AreaThickness.ThicknessMembrane <= 0 || Bad(plate.AreaThickness.ThicknessMembrane) || Bad(plate.AreaThickness.ThicknessBending) || (plate.AreaProperty?.PropertyType==AreaPropertyModel.PlatePropertyType.PlateShell && plate.AreaThickness.ThicknessBending <= 0), $"Plate {plate.Id}: thickness must be positive.");
            if (plate.NodeList?.Count >= 3)
                Error(Vector3d.CrossProduct(plate.NodeList[1].Position - plate.NodeList[0].Position, plate.NodeList[2].Position - plate.NodeList[0].Position).Length <= tol * tol, $"Plate {plate.Id}: degenerate first three nodes.");
            if(plate.NodeList?.Count==4)
            {
                var a=plate.NodeList[0].Position;var b=plate.NodeList[1].Position;var c=plate.NodeList[2].Position;
                var normal=Vector3d.CrossProduct(b-a,c-a);normal.Unitize();
                Error(Math.Abs(Vector3d.Multiply(plate.NodeList[3].Position-a,normal))>tol,$"Plate {plate.Id}: non-planar Quad4; triangulate it with Mesh To Plates.");
            }
            foreach (var load in plate.AreaLoadList)
            {
                Error(load.LoadCase == null, $"Plate {plate.Id}: missing load case.");
                if (load is AreaPressureLoadModel p) Error(Bad(p.Value), $"Plate {plate.Id}: invalid pressure.");
                else if (load is AreaGlobalPressureStraus g) Error(BadVector(g.Value) || !Enum.IsDefined(g.Face), $"Plate {plate.Id}: invalid global pressure.");
                else if (load is AreaTemperatureGradientModel t) Error(Bad(t.Gradient), $"Plate {plate.Id}: invalid gradient.");
                else if (load is AreaPreloadModel p2) Error(BadVector(p2.Value), $"Plate {plate.Id}: invalid preload.");
                else if (load is AreaEdgePressureModel e) Error(Bad(e.Value) || e.Edge < 1 || e.Edge > plate.NodeList.Count, $"Plate {plate.Id}: invalid edge pressure.");
                else v.Errors.Add($"Plate {plate.Id}: unsupported load {load.GetType().Name}.");
            }
            Error(plate.AreaLoadList.GroupBy(x => (x.GetType(), x.LoadCase?.Name, x is AreaEdgePressureModel e ? e.Edge : 0, x is AreaGlobalPressureStraus g ? (int)g.Face : 0)).Any(g => g.Count() > 1), $"Plate {plate.Id}: duplicate load attribute in one case; combine values first.");
        }
        foreach(var solid in model.SolidElements.Values)
        {
            Error(solid.Material==null,$"Solid {solid.Id}: missing material.");
            if(solid.Nodes.Count is not (4 or 6 or 8)){v.Errors.Add($"Solid {solid.Id}: use Tet4, Wedge6 or Hexa8.");continue;}
            Error(solid.Nodes.Select(n=>n.Id).Distinct().Count()!=solid.Nodes.Count||solid.SignedVolume()<=tol*tol*tol,$"Solid {solid.Id}: degenerate or inverted connectivity.");
            int faces=solid.Nodes.Count==4?4:solid.Nodes.Count==6?5:6;
            foreach(var p in solid.Pressures)Error(p.LoadCase==null||Bad(p.Value)||p.Face<1||p.Face>faces,$"Solid {solid.Id}: invalid face pressure.");
        }
        foreach (var link in model.LinkElements.Values)
        {
            Error(link.LinkProperty == null || !Enum.IsDefined(link.LinkProperty.Type), $"Link {link.Id}: missing or invalid property.");
            var p=link.LinkProperty;
            if(p!=null)Error(new[]{p.Kx,p.Ky,p.Kz,p.Rx,p.Ry,p.Rz}.Any(x=>Bad(x)||x<0), $"Link {link.Id}: stiffness must be finite and nonnegative.");
            Error(link.NodeStart==null||link.NodeEnd==null||ReferenceEquals(link.NodeStart,link.NodeEnd),$"Link {link.Id}: needs two distinct model nodes.");
        }
        foreach (var combo in model.LoadCombinations.Values)
        {
            Error(combo.Type != LoadCombinationModel.LoadCombinationTypes.Linear, $"Combination {combo.Name}: only linear combinations are supported.");
            Error(combo.LoadFactorList == null || combo.LoadFactorList.Count == 0 || combo.LoadFactorList.Any(x => x.LoadCase == null || Bad(x.Factor)), $"Combination {combo.Name}: invalid or empty factors.");
        }
        if (model.SelfWeight != null) Error(model.SelfWeight.LoadCase == null || Bad(model.SelfWeight.GravityValue) || model.SelfWeight.GlobalInertiaLoad != SelfWeightModel.GlobalInertiaLoads.Gravity, "Self weight requires a gravity load case and finite acceleration.");
        if (!model.NodeElements.Values.Any(x => x.Support?.IsActive == true || x.TranslationSpring != Vector3d.Zero)) v.Warnings.Add("No restraints or translation springs. Check stability before solving.");
        return v;
    }
}
