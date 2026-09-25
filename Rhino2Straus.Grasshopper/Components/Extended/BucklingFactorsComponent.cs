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

public sealed class BucklingFactorsComponent : SafeComponent
{
    public BucklingFactorsComponent() : base("Buckling Factors", "Buckling Factors", "Buckling Factors", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("2fddf0be-6282-52ae-aa36-5fc5a6e5fe51");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Model path", "Model path", "Model path", GH_ParamAccess.item);
        p.AddTextParameter("Result path", "Result path", "Result path", GH_ParamAccess.item);
        p.AddIntegerParameter("Modes", "Modes", "Modes", GH_ParamAccess.list);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddNumberParameter("Factors", "Factors", "Factors", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var modes=new List<int>();da.GetDataList(2,modes);da.SetDataList(0,StrausFileService.BucklingFactors(Item<string>(da,0),Item<string>(da,1),modes.ToArray()));
    }
}
