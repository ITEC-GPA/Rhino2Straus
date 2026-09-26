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

public sealed class PlateMomentResultsComponent : SafeComponent
{
    public PlateMomentResultsComponent() : base("Plate Moments", "Plate Moments", "Plate Moments", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("7b4fe87b-63e1-5669-8638-bb3f32bc00e4");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Model path", "Model path", "Existing .st7 model path", GH_ParamAccess.item);
        p.AddTextParameter("Result path", "Result path", "Existing .LSA or compatible Straus7 result file", GH_ParamAccess.item);
        p.AddIntegerParameter("Case", "Case", "Result case number starting at 1", GH_ParamAccess.item, 1);
        p.AddIntegerParameter("Plate number", "Plate number", "Plate number", GH_ParamAccess.item, 1);
        p.AddIntegerParameter("Surface", "Surface", "0=midplane, 1=bottom (-Z), 2=top (+Z)", GH_ParamAccess.item, 0);
        p.AddIntegerParameter("Sample", "Sample", "0=centroid, 1=Gauss, 2=nodes unaveraged, 3=average all, 4=average same", GH_ParamAccess.item, 0);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddNumberParameter("Values", "Values", "One branch per sample; column order is the Straus7 API order, local plate axes", GH_ParamAccess.tree);
        p.AddIntegerParameter("Samples", "Samples", "Samples", GH_ParamAccess.item);
        p.AddIntegerParameter("Columns", "Columns", "Columns", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var t=StrausFileService.PlateResults(Item<string>(da,0),Item<string>(da,1),Item<int>(da,2),Item<int>(da,3),St7.rtPlateMoment,Item<int>(da,4),Item<int>(da,5));
        var tree=new GH_Structure<GH_Number>(); for(int i=0;i<t.Values.Length;i++)tree.AppendRange(t.Values[i].Select(x=>new GH_Number(x)),new GH_Path(i)); da.SetDataTree(0,tree); da.SetData(1,t.Values.Length); da.SetData(2,t.Values.Length==0?0:t.Values[0].Length);
    }
}
