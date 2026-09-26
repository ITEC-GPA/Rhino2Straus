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

public sealed class BeamForcesComponent : SafeComponent
{
    public BeamForcesComponent() : base("Beam Forces", "Beam Forces", "Beam Forces", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("9d88e678-cb9c-5dd5-8a8d-844bba34db88");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Model path", "Model path", "Existing .st7 model path", GH_ParamAccess.item);
        p.AddTextParameter("Result path", "Result path", "Existing .LSA or compatible Straus7 result file", GH_ParamAccess.item);
        p.AddIntegerParameter("Case", "Case", "Result case number starting at 1", GH_ParamAccess.item, 1);
        p.AddIntegerParameter("Beam number", "Beam number", "Beam number", GH_ParamAccess.item, 1);
        p.AddIntegerParameter("Stations", "Stations", "Minimum station count, 2..100", GH_ParamAccess.item, 11);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddNumberParameter("Position", "Position", "Distance from end 1 in model length units", GH_ParamAccess.list);
        p.AddNumberParameter("N", "N", "N", GH_ParamAccess.list);
        p.AddNumberParameter("V1", "V1", "V1", GH_ParamAccess.list);
        p.AddNumberParameter("V2", "V2", "V2", GH_ParamAccess.list);
        p.AddNumberParameter("T", "T", "T", GH_ParamAccess.list);
        p.AddNumberParameter("M1", "M1", "M1", GH_ParamAccess.list);
        p.AddNumberParameter("M2", "M2", "M2", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var t=StrausFileService.BeamResults(Item<string>(da,0),Item<string>(da,1),Item<int>(da,2),Item<int>(da,3),St7.rtBeamForce,St7.stBeamPrincipal,Item<int>(da,4));
        da.SetDataList(0,t.Positions); int[] columns={St7.ipBeamAxialF,St7.ipBeamSF1,St7.ipBeamSF2,St7.ipBeamTorque,St7.ipBeamBM1,St7.ipBeamBM2};
        for(int i=0;i<columns.Length;i++){int c=columns[i];da.SetDataList(i+1,t.Values.Select(row=>row[c]));}
    }
}
