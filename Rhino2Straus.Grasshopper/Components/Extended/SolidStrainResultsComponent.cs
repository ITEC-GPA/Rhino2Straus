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

public sealed class SolidStrainResultsComponent : SafeComponent
{
    public SolidStrainResultsComponent() : base("Solid Strain", "Solid Strain", "Solid Strain", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("ff170307-4a7f-5ad5-ae8d-03b517314c26");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Model path", "Model path", "Model path", GH_ParamAccess.item);
        p.AddTextParameter("Result path", "Result path", "Result path", GH_ParamAccess.item);
        p.AddIntegerParameter("Case", "Case", "Case", GH_ParamAccess.item, 1);
        p.AddIntegerParameter("Solid number", "Solid number", "Solid number", GH_ParamAccess.item, 1);
        p.AddIntegerParameter("Sample", "Sample", "0=centroid, 1=Gauss, 2=nodes unaveraged, 3=nodes all, 4=nodes same property", GH_ParamAccess.item, 0);
        p.AddBooleanParameter("Combined", "Combined", "False: global XX YY ZZ XY YZ ZX. True: native principal/equivalent measures", GH_ParamAccess.item, false);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddNumberParameter("Values", "Values", "Branch per sample, native Straus7 column order", GH_ParamAccess.tree);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var result=StrausFileService.SolidResults(Item<string>(da,0),Item<string>(da,1),Item<int>(da,2),Item<int>(da,3),St7.rtBrickStrain,Item<int>(da,4),Item<bool>(da,5));
        var tree=new DataTree<double>();for(int i=0;i<result.Values.Length;i++)tree.AddRange(result.Values[i],new GH_Path(i));da.SetDataTree(0,tree);
    }
}
