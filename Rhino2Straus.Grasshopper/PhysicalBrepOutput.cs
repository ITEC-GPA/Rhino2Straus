using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino2Straus.Core.Helper;

namespace Rhino2Straus.Grasshopper;

internal static class PhysicalBrepOutput
{
    internal const string Description="Closed physical volumes for native Rhino bake; branches {kind; Model ID}, kind 0=beam, 1=plate, 2=solid. Preserves offsets and thickness. Independent of visibility; no boolean union.";
    internal static GH_Structure<GH_Brep> Tree(ModelBrepGeometry geometry)
    {
        var tree=new GH_Structure<GH_Brep>();
        foreach(var element in geometry.Elements)
            tree.Append(new GH_Brep(element.Geometry),new GH_Path(element.Kind,element.ElementId));
        return tree;
    }
    internal static void Bake(GH_Component component,RhinoDoc document,ObjectAttributes attributes,List<Guid> objectIds)
    {
        if(document==null)throw new ArgumentNullException(nameof(document));
        if(objectIds==null)throw new ArgumentNullException(nameof(objectIds));
        var breps=component.Params.Output[4].VolatileData.AllData(true).OfType<GH_Brep>().Select(b=>b.Value).ToArray();
        if(breps.Any(b=>b==null||!b.IsValid||!b.IsSolid))throw new InvalidOperationException("Bake requires valid closed Breps. Recompute the component.");
        var created=new List<Guid>();uint undo=document.BeginUndoRecord("Bake Rhino2Straus solids");
        try
        {
            foreach(var brep in breps)
            {
                var att=attributes?.Duplicate()??document.CreateDefaultAttributes();
                foreach(var key in new[]{"Rhino2Straus.ElementType","Rhino2Straus.ElementId","Rhino2Straus.PartIndex"})
                    if(brep.GetUserString(key) is {} value)att.SetUserString(key,value);
                if(string.IsNullOrEmpty(att.Name))att.Name=$"Rhino2Straus {brep.GetUserString("Rhino2Straus.ElementType")} {brep.GetUserString("Rhino2Straus.ElementId")}";
                var id=document.Objects.AddBrep(brep,att);
                if(id==Guid.Empty)throw new InvalidOperationException("Rhino rejected a solid; this bake was rolled back.");
                created.Add(id);
            }
            objectIds.AddRange(created);
        }
        catch
        {
            foreach(var id in created)document.Objects.Delete(id,true);
            throw;
        }
        finally
        {
            if(undo!=0)document.EndUndoRecord(undo);
            document.Views.Redraw();
        }
    }
}
