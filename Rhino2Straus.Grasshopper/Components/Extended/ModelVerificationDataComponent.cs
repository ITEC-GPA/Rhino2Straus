using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Loads;
using Rhino2Straus.Core.Models;
using Rhino2Straus.Core.Settings;
using Rhino2Straus.Grasshopper.Datatype;
using Rhino2Straus.Import;
using St7API;

namespace Rhino2Straus.Grasshopper.Components.Extended;

public sealed class ModelVerificationDataComponent : SafeComponent
{
    public ModelVerificationDataComponent() : base("Model Verification Data", "Model Verification Data", "Data contract for a future structural verifier. This component does not perform design/code checks. Inspect unavailable quantities for each check.", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("5273ffd9-0efd-5489-8f62-f8ac21c33eb2");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        p.AddBooleanParameter("Require provenance", "Require provenance", "Refuse result files without verified input/result hashes", GH_ParamAccess.item, true);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Elements", "Elements", "Paths {entity type;entity ID}; enum Node=0 Beam=1 Plate=2 Solid=3 Link=4", GH_ParamAccess.tree);
        p.AddGenericParameter("Properties", "Properties", "Properties", GH_ParamAccess.tree);
        p.AddGenericParameter("Materials", "Materials", "Materials", GH_ParamAccess.tree);
        p.AddGenericParameter("Results", "Results", "Immutable tables carrying case, quantity, axes, surface and samples", GH_ParamAccess.tree);
        p.AddGenericParameter("Units", "Units", "Units", GH_ParamAccess.item);
        p.AddTextParameter("Unavailable", "Unavailable", "Unavailable", GH_ParamAccess.list);
        p.AddGenericParameter("Native sections", "Native sections", "Actual Straus7 section properties; indexed by Model beam property ID", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var m=Item<GH_Model>(da,0).Value;var r=m.Results??throw new InvalidOperationException("Model has no results.");r.ValidateFor(m,requireProvenance:Item<bool>(da,1));
        var elements=new GH_Structure<GH_ObjectWrapper>();var properties=new GH_Structure<GH_ObjectWrapper>();var materials=new GH_Structure<GH_ObjectWrapper>();var results=new GH_Structure<GH_ObjectWrapper>();
        var byElement=r.Records.ToLookup(x=>(x.Entity,x.EntityId));
        void Add(ResultEntity kind,int id,object element,object property,object material){var path=new GH_Path((int)kind,id);elements.Append(new GH_ObjectWrapper(element),path);if(property!=null)properties.Append(new GH_ObjectWrapper(property),path);if(material!=null)materials.Append(new GH_ObjectWrapper(material),path);results.AppendRange(byElement[(kind,id)].Select(x=>new GH_ObjectWrapper(x)),path);}
        foreach(var x in m.NodeElements.Values)Add(ResultEntity.Node,x.Id,x,x.Support,null);foreach(var x in m.FrameElements.Values)Add(ResultEntity.Beam,x.Id,x,x.FrameProperty,x.Material);foreach(var x in m.AreaElements.Values)Add(ResultEntity.Plate,x.Id,x,x.AreaProperty,x.Material);foreach(var x in m.SolidElements.Values)Add(ResultEntity.Solid,x.Id,x,null,x.Material);foreach(var x in m.LinkElements.Values)Add(ResultEntity.Link,x.Id,x,x.LinkProperty,null);
        da.SetDataTree(0,elements);da.SetDataTree(1,properties);da.SetDataTree(2,materials);da.SetDataTree(3,results);da.SetData(4,new GH_Units(m.ModelUnits));da.SetDataList(5,r.Issues.Select(x=>x.ToString()));
        da.SetDataList(6,r.BeamSections.Select(x=>new GH_ObjectWrapper(x)));
    }
}
