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

public sealed class BeamPreloadComponent : SafeComponent
{
    public BeamPreloadComponent() : base("Beam Preload", "Beam Preload", "Beam Preload", "Rhino2Straus", "06-Loads") { }
    public override Guid ComponentGuid => new Guid("c4cfee95-8df4-58ce-b6e9-e1a068bde057");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
        p.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
        p.AddNumberParameter("Value", "Value", "Pretension force, or dimensionless prestrain", GH_ParamAccess.item, 0.0);
        p.AddBooleanParameter("Strain", "Strain", "Strain", GH_ParamAccess.item, false);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var b=new FrameElementModel(Item<GH_FrameElement>(da,0).Value);
        b.FrameLoadList.Add(new FramePreloadModel { LoadCase=Item<GH_LoadCase>(da,1).Value,Value=Item<double>(da,2),Strain=Item<bool>(da,3) }); da.SetData(0,new GH_FrameElement(b));
    }
}
