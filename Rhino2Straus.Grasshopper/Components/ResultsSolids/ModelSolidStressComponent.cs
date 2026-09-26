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

public sealed class ModelSolidStressComponent : SafeComponent
{
    public ModelSolidStressComponent() : base("Model Solid Stress", "Model Solid Stress", "Model Solid Stress", ComponentTopics.Tab, ComponentTopics.ResultsSolids) { }
    public override Guid ComponentGuid => new Guid("6ea9d327-e5c2-5fcc-b3e7-600059eed65f");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        p.AddIntegerParameter("Case", "Case", "0 all result cases; otherwise result case number (not load case ID)", GH_ParamAccess.item, 0);
        p.AddIntegerParameter("IDs", "IDs", "Model entity IDs; empty means all", GH_ParamAccess.list);
        p[2].Optional = true;
        p.AddIntegerParameter("Surface", "Surface", "-1 all; 0 mid-plane; 1 minus Z; 2 plus Z", GH_ParamAccess.item, -1);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddNumberParameter("Values", "Values", "Paths {case;entity ID;surface;sample}, items in Columns order", GH_ParamAccess.tree);
        p.AddNumberParameter("Positions", "Positions", "Beam distances or sample indices. Paths {case;entity ID;surface}", GH_ParamAccess.tree);
        p.AddTextParameter("Columns", "Columns", "Columns", GH_ParamAccess.tree);
        p.AddTextParameter("Axes and sampling", "Axes and sampling", "Axes and sampling", GH_ParamAccess.tree);
        p.AddGenericParameter("Records", "Records", "Typed immutable records for downstream verification", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var m=Item<GH_Model>(da,0).Value;var r=m.Results??throw new InvalidOperationException("Model has no results.");
        var ids=new List<int>();da.GetDataList(2,ids);int surface=Item<int>(da,3);if(surface < -1 || surface>2)throw new ArgumentException("Surface must be -1..2.");
        var records=r.Select(m,ResultEntity.Solid,ResultQuantity.Stress,Item<int>(da,1),ids,surface).ToArray();
        ResultTrees.Write(da,records);if(records.Length==0)AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,"No matching result tables; inspect Model Result Cases / Unavailable.");
    }
}
