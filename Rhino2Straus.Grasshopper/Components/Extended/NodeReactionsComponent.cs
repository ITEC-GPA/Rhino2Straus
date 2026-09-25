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

public sealed class NodeReactionsComponent : SafeComponent
{
    public NodeReactionsComponent() : base("Node Reactions", "Node Reactions", "Node Reactions", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("1c4c28dd-ca27-5495-af23-b4495914f76b");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Model path", "Model path", "Existing .st7 model path", GH_ParamAccess.item);
        p.AddTextParameter("Result path", "Result path", "Existing .LSA or compatible Straus7 result file", GH_ParamAccess.item);
        p.AddIntegerParameter("Case", "Case", "Result case number starting at 1", GH_ParamAccess.item, 1);
        p.AddIntegerParameter("Node numbers", "Node numbers", "Straus7 internal numbers, not user labels", GH_ParamAccess.list);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddVectorParameter("Force", "Force", "Force", GH_ParamAccess.list);
        p.AddVectorParameter("Moment", "Moment", "Moment", GH_ParamAccess.list);
        p.AddNumberParameter("Magnitude", "Magnitude", "Magnitude of first vector", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var numbers=new List<int>(); da.GetDataList(3,numbers); if(numbers.Count==0)throw new ArgumentException("Supply node numbers.");
        var rows=StrausFileService.NodeResults(Item<string>(da,0),Item<string>(da,1),Item<int>(da,2),numbers,true);
        var v=rows.Select(x=>new Vector3d(x[0],x[1],x[2])).ToArray(); da.SetDataList(0,v); da.SetDataList(1,rows.Select(x=>new Vector3d(x[3],x[4],x[5]))); da.SetDataList(2,v.Select(x=>x.Length));
    }
}
