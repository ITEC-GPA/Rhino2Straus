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

public sealed class ResultEnvelopeComponent : SafeComponent
{
    public ResultEnvelopeComponent() : base("Result Envelope", "Result Envelope", "Result Envelope", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("d2806b8c-e9e0-5843-b52b-997a2155f056");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddNumberParameter("Cases", "Cases", "Each tree branch contains one case with identically ordered samples", GH_ParamAccess.tree);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddNumberParameter("Minimum", "Minimum", "Minimum", GH_ParamAccess.list);
        p.AddNumberParameter("Maximum", "Maximum", "Maximum", GH_ParamAccess.list);
        p.AddIntegerParameter("Min branch", "Min branch", "Zero-based branch index giving minimum", GH_ParamAccess.list);
        p.AddIntegerParameter("Max branch", "Max branch", "Zero-based branch index giving maximum", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        GH_Structure<GH_Number> tree; if(!da.GetDataTree(0,out tree)||tree.PathCount==0)throw new ArgumentException("Supply result cases as a tree.");
        int count=tree.Branches[0].Count; if(count==0 || tree.Branches.Any(b=>b.Count!=count || b.Any(x=>!double.IsFinite(x.Value))))throw new ArgumentException("All branches must have matching lengths and finite values.");
        var min=new List<double>();var max=new List<double>();var imin=new List<int>();var imax=new List<int>();
        for(int j=0;j<count;j++){var values=tree.Branches.Select(b=>b[j].Value).ToArray();double lo=values.Min(),hi=values.Max();min.Add(lo);max.Add(hi);imin.Add(Array.IndexOf(values,lo));imax.Add(Array.IndexOf(values,hi));}
        da.SetDataList(0,min);da.SetDataList(1,max);da.SetDataList(2,imin);da.SetDataList(3,imax);
    }
}
