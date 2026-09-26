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

public sealed class AlignPlateAxisComponent : SafeComponent
{
    public AlignPlateAxisComponent() : base("Align Plate Local Axis", "Align Plate Local Axis", "Align a local axis to a projected direction. Local loads and offsets follow the rotated axes. Input stays unchanged.", ComponentTopics.Tab, ComponentTopics.AxesOffsets) { }
    public override Guid ComponentGuid => new Guid("653d49e0-25b6-54f5-ab7f-e7c959f09e58");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);
        p.AddVectorParameter("Direction", "Direction", "Desired direction projected into the transverse beam plane / plate plane", GH_ParamAccess.item, Vector3d.XAxis);
        p.AddIntegerParameter("Axis", "Axis", "Axis 1 or 2", GH_ParamAccess.item, 1);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);
        p.AddPlaneParameter("Axes", "Axes", "Axes", GH_ParamAccess.item);
        p.AddNumberParameter("Angle", "Angle", "Final orientation in radians", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var element=ElementAlignment.Plate(Item<GH_AreaElement>(da,0).Value,Item<Vector3d>(da,1),Item<int>(da,2));
        element.GetLocalAxes(out var x,out var y,out var z); var origin=element.NodeList[0].Position;
        da.SetData(0,new GH_AreaElement(element));da.SetData(1,new Plane(origin,x,y));da.SetData(2,element.Angle);
    }
}
