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

public sealed class BeamGradientComponent : SafeComponent
{
    public BeamGradientComponent() : base("Beam Temperature Gradient", "Beam Temperature Gradient", "Beam Temperature Gradient", "Rhino2Straus", "06-Loads") { }
    public override Guid ComponentGuid => new Guid("1066a017-d9eb-5713-ba1f-a670614db109");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
        p.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
        p.AddNumberParameter("Gradient 1", "Gradient 1", "Temperature difference per model length in principal axis 1", GH_ParamAccess.item, 0.0);
        p.AddNumberParameter("Gradient 2", "Gradient 2", "Temperature difference per model length in principal axis 2", GH_ParamAccess.item, 0.0);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var b=new FrameElementModel(Item<GH_FrameElement>(da,0).Value);
        b.FrameLoadList.Add(new FrameTemperatureGradientModel { LoadCase=Item<GH_LoadCase>(da,1).Value,Gradient1=Item<double>(da,2),Gradient2=Item<double>(da,3) }); da.SetData(0,new GH_FrameElement(b));
    }
}
