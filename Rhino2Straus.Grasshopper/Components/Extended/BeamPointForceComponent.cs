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

public sealed class BeamPointForceComponent : SafeComponent
{
    public BeamPointForceComponent() : base("Beam Point Force", "Beam Point Force", "Beam Point Force", "Rhino2Straus", "06-Loads") { }
    public override Guid ComponentGuid => new Guid("6557d511-938c-590d-8630-fefa669804f0");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
        p.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
        p.AddVectorParameter("Value", "Value", "XYZ global or principal 1,2,3 components", GH_ParamAccess.item);
        p.AddNumberParameter("Position", "Position", "Relative distance from end 1, 0..1", GH_ParamAccess.item, 0.5);
        p.AddBooleanParameter("Global", "Global", "Global", GH_ParamAccess.item, true);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var b=new FrameElementModel(Item<GH_FrameElement>(da,0).Value); double p=Item<double>(da,3); var v=Item<Vector3d>(da,2);
        if (!v.IsValid || !double.IsFinite(p) || p<0 || p>1) throw new ArgumentException("Use a finite vector and a position between 0 and 1.");
        b.FrameLoadList.Add(new FramePointLoadModel { LoadCase=Item<GH_LoadCase>(da,1).Value, Value=v, Position=p, Moment=false, CoordinateSystem=Item<bool>(da,4)?CoordinateSystemModel.Global:CoordinateSystemModel.Local });
        da.SetData(0,new GH_FrameElement(b));
    }
}
