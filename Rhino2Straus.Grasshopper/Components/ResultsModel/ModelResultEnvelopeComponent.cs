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

public sealed class ModelResultEnvelopeComponent : SafeComponent
{
    public ModelResultEnvelopeComponent() : base("Model Result Extrema", "Model Result Extrema", "Independent signed minima/maxima with governing case and position. Extrema of different quantities are not a simultaneous load state.", ComponentTopics.Tab, ComponentTopics.ResultsModel) { }
    public override Guid ComponentGuid => new Guid("eb9d1d5d-f494-5774-a677-3173ee4a1bca");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        p.AddTextParameter("Entity", "Entity", "Node, Beam, Plate, Solid or Link", GH_ParamAccess.item, "Beam");
        p.AddTextParameter("Quantity", "Quantity", "Force, Moment, Stress, Strain, Displacement, Reaction, ...", GH_ParamAccess.item, "Force");
        p.AddTextParameter("Column", "Column", "Column name, e.g. N, M1, XX or VonMises", GH_ParamAccess.item, "N");
        p.AddIntegerParameter("Cases", "Cases", "Empty means all", GH_ParamAccess.list);
        p[4].Optional = true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddNumberParameter("Min Max", "Min Max", "Two values per {entity ID;surface}, across selected cases AND samples", GH_ParamAccess.tree);
        p.AddIntegerParameter("Governing cases", "Governing cases", "Governing cases", GH_ParamAccess.tree);
        p.AddNumberParameter("Governing positions", "Governing positions", "Governing positions", GH_ParamAccess.tree);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var m=Item<GH_Model>(da,0).Value;var r=m.Results??throw new InvalidOperationException("Model has no results.");
        if(!Enum.TryParse<ResultEntity>(Item<string>(da,1),true,out var entity)||!Enum.IsDefined(entity)||!Enum.TryParse<ResultQuantity>(Item<string>(da,2),true,out var quantity)||!Enum.IsDefined(quantity))throw new ArgumentException("Unknown entity or quantity.");
        string column=Item<string>(da,3);var cases=new List<int>();da.GetDataList(4,cases);if(cases.Except(r.Cases.Select(x=>x.Number)).Any())throw new ArgumentException("Unknown result case.");
        var values=new GH_Structure<GH_Number>();var governing=new GH_Structure<GH_Integer>();var positions=new GH_Structure<GH_Number>();
        foreach(var group in r.Select(m,entity,quantity).Where(x=>cases.Count==0||cases.Contains(x.CaseNumber)).GroupBy(x=>(x.EntityId,x.Surface)))
        {var candidates=group.SelectMany(x=>{int col=x.Columns.IndexOf(column);if(col<0)throw new ArgumentException("Unknown column "+column);return x.Values.Select((v,i)=>(Value:v[col],Case:x.CaseNumber,Position:x.Positions[i]));}).ToArray();
         var path=new GH_Path(group.Key.EntityId,group.Key.Surface);foreach(var extreme in new[]{candidates.MinBy(x=>x.Value),candidates.MaxBy(x=>x.Value)}){values.Append(new GH_Number(extreme.Value),path);governing.Append(new GH_Integer(extreme.Case),path);positions.Append(new GH_Number(extreme.Position),path);}}
        da.SetDataTree(0,values);da.SetDataTree(1,governing);da.SetDataTree(2,positions);
    }
}
