using System;
using System.Collections.Generic;
using System.Linq;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.Settings;

namespace Rhino2Straus.Core.Models;

public static class ModelComposer
{
    public static ModelModel Merge(IEnumerable<ModelModel> models, bool renumber = false)
    {
        var parts=models.ToList();
        if(parts.Count==0 || parts.Any(x=>x==null))throw new ArgumentException("Supply at least one model.");
        var units=parts[0].ModelUnits;
        if(parts.Any(x=>!SameUnits(units,x.ModelUnits)))throw new ArgumentException("Models have different units. Convert them before merging.");
        var solids=new List<SolidElementModel>(); var nodes=new List<NodeElementModel>();var beams=new List<FrameElementModel>();var plates=new List<AreaElementModel>();var links=new List<LinkElementModel>();var combos=new List<LoadCombinationModel>();
        SelfWeightModel weight=null;
        foreach(var source in parts)
        {
            var m=new ModelModel(source);
            if(m.SelfWeight!=null)
            {
                if(weight!=null && (weight.LoadCase.Name!=m.SelfWeight.LoadCase.Name || weight.GravityValue!=m.SelfWeight.GravityValue || weight.GravityDirection!=m.SelfWeight.GravityDirection || weight.StructuralMassAcceleration!=m.SelfWeight.StructuralMassAcceleration || weight.NonStructuralMassAcceleration!=m.SelfWeight.NonStructuralMassAcceleration))throw new ArgumentException("Models contain incompatible self-weight definitions.");
                weight=m.SelfWeight;
            }
            if(renumber)
            {
                foreach(var s in m.SolidElements.Values)s.Id=-1;foreach(var n in m.NodeElements.Values)n.Id=-1;foreach(var b in m.FrameElements.Values)b.Id=-1;foreach(var p in m.AreaElements.Values)p.Id=-1;foreach(var l in m.LinkElements.Values)l.Id=-1;
                foreach(var x in m.Materials.Values)x.Id=-1;foreach(var x in m.FrameProperties.Values)x.Id=-1;foreach(var x in m.FrameSections.Values)x.Id=-1;foreach(var x in m.AreaProperties.Values)x.Id=-1;foreach(var x in m.AreaThicknesses.Values)x.Id=-1;
            }
            solids.AddRange(m.SolidElements.Values);nodes.AddRange(m.NodeElements.Values);beams.AddRange(m.FrameElements.Values);plates.AddRange(m.AreaElements.Values);links.AddRange(m.LinkElements.Values);
            foreach(var c in m.LoadCombinations.Values)
            {
                var existing=combos.FirstOrDefault(x=>x.Name==c.Name);
                if(existing==null)combos.Add(c);
                else if(existing.Type!=c.Type || !existing.LoadFactorList.Select(x=>(x.LoadCase.Name,x.Factor)).OrderBy(x=>x.Name).SequenceEqual(c.LoadFactorList.Select(x=>(x.LoadCase.Name,x.Factor)).OrderBy(x=>x.Name)))throw new ArgumentException($"Conflicting load combination {c.Name}.");
            }
        }
        var result=new ModelModel {ModelUnits=new ModelUnitsModel(units)};result.BuildModel(nodes,beams,plates,links,combos,weight,solids);return result;
    }
    public static bool SameUnits(ModelUnitsModel a,ModelUnitsModel b)=>a!=null&&b!=null&&a.LengthUnit==b.LengthUnit&&a.ForceUnit==b.ForceUnit&&a.HeatUnit==b.HeatUnit&&a.TemperatureUnit==b.TemperatureUnit;
}
