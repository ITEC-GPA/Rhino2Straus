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

public sealed class BeamReleasesComponent : SafeComponent
{
    public BeamReleasesComponent() : base("Beam End Releases", "Beam End Releases", "Beam End Releases", "Rhino2Straus", "04-Attributes") { }
    public override Guid ComponentGuid => new Guid("3a5844a8-705f-51ec-99fd-7bfb7d7e7752");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
        p.AddBooleanParameter("Start", "Start", "Six flags T1,T2,T3,R1,R2,R3; true releases the DOF", GH_ParamAccess.list);
        p.AddBooleanParameter("End", "End", "Six flags T1,T2,T3,R1,R2,R3; true releases the DOF", GH_ParamAccess.list);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var b=new FrameElementModel(Item<GH_FrameElement>(da,0).Value); var start=new List<bool>(); var end=new List<bool>();
        da.GetDataList(1,start); da.GetDataList(2,end); if(start.Count!=6 || end.Count!=6) throw new ArgumentException("Supply exactly six release flags per end.");
        b.StartReleases=start.ToArray(); b.EndReleases=end.ToArray(); da.SetData(0,new GH_FrameElement(b));
    }
}
