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

public sealed class PolylineBeamsComponent : SafeComponent
{
    public PolylineBeamsComponent() : base("Polyline to Beams", "Polyline to Beams", "Polyline to Beams", "Rhino2Straus", "03-Elements") { }
    public override Guid ComponentGuid => new Guid("48953e07-25f6-5e5e-aa5a-60b94abd8f08");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddCurveParameter("Polyline", "Polyline", "Piecewise linear curve; each segment becomes a beam", GH_ParamAccess.item);
        p.AddGenericParameter("Property", "Property", "Property", GH_ParamAccess.item);
        p.AddIntegerParameter("First ID", "First ID", "First ID", GH_ParamAccess.item, -1);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Beams", "Beams", "Beams", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var curve=Item<Curve>(da,0); if(!curve.TryGetPolyline(out var polyline)) throw new ArgumentException("Supply a polyline, not a curved spline.");
        var property=Item<GH_FrameProperty>(da,1).Value; int first=Item<int>(da,2); if(first==0 || first < -1) throw new ArgumentException("Invalid first ID.");
        var beams=new List<GH_FrameElement>(); for(int i=0;i<polyline.SegmentCount;i++) { var segment=polyline.SegmentAt(i);
            if(segment.Length<=1e-12) throw new ArgumentException("Polyline contains a zero-length segment.");
            beams.Add(new GH_FrameElement(new FrameElementModel(new NodeElementModel(segment.From),new NodeElementModel(segment.To),property,0,Vector3d.Zero){Id=first<0?-1:checked(first+i)})); }
        da.SetDataList(0,beams);
    }
}
