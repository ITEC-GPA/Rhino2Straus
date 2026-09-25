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

public sealed class ModelBeamActionsComponent : SafeComponent
{
    public ModelBeamActionsComponent() : base("Model Beam Actions N V T M", "Model Beam Actions N V T M", "Model Beam Actions N V T M", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("dc675280-e907-5e4e-8fe1-fcdeff747e66");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        p.AddIntegerParameter("Case", "Case", "Case", GH_ParamAccess.item, 0);
        p.AddIntegerParameter("IDs", "IDs", "IDs", GH_ParamAccess.list);
        p[2].Optional = true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddNumberParameter("N", "N", "Axial force", GH_ParamAccess.tree);
        p.AddNumberParameter("V1", "V1", "V1", GH_ParamAccess.tree);
        p.AddNumberParameter("V2", "V2", "V2", GH_ParamAccess.tree);
        p.AddNumberParameter("T", "T", "Torsion", GH_ParamAccess.tree);
        p.AddNumberParameter("M1", "M1", "M1", GH_ParamAccess.tree);
        p.AddNumberParameter("M2", "M2", "M2", GH_ParamAccess.tree);
        p.AddNumberParameter("Positions", "Positions", "Distance from end 1; all outputs paths {case;beam ID}", GH_ParamAccess.tree);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var m=Item<GH_Model>(da,0).Value;var r=m.Results??throw new InvalidOperationException("Model has no results.");var ids=new List<int>();da.GetDataList(2,ids);
        var trees=Enumerable.Range(0,7).Select(_=>new GH_Structure<GH_Number>()).ToArray();string[] names={"N","V1","V2","T","M1","M2"};
        foreach(var record in r.Select(m,ResultEntity.Beam,ResultQuantity.Force,Item<int>(da,1),ids))
        {var path=new GH_Path(record.CaseNumber,record.EntityId);for(int i=0;i<6;i++){int col=record.Columns.IndexOf(names[i]);if(col<0)throw new InvalidOperationException("Missing beam action "+names[i]);trees[i].AppendRange(record.Values.Select(v=>new GH_Number(v[col])),path);}trees[6].AppendRange(record.Positions.Select(p=>new GH_Number(p)),path);}
        for(int i=0;i<7;i++)da.SetDataTree(i,trees[i]);
    }
}
