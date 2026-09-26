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

public sealed class AlignPlateToPlaneComponent : SafeComponent
{
    public AlignPlateToPlaneComponent() : base("Align Plate Axis To Plane", "Align Plate Axis To Plane", "Align Plate Axis To Plane", ComponentTopics.Tab, ComponentTopics.AxesOffsets) { }
    public override Guid ComponentGuid => new Guid("75aa6043-c636-5975-be82-92bbe028458d");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);
        p.AddPlaneParameter("Reference", "Reference", "Reference", GH_ParamAccess.item, Plane.WorldXY);
        p.AddIntegerParameter("Axis", "Axis", "Axis", GH_ParamAccess.item, 1);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);
        p.AddPlaneParameter("Axes", "Axes", "Axes", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var reference=Item<Plane>(da,1);if(!reference.IsValid)throw new ArgumentException("Invalid reference plane.");
        var aligned=ElementAlignment.Plate(Item<GH_AreaElement>(da,0).Value,reference.XAxis,Item<int>(da,2));
        aligned.GetLocalAxes(out var x,out var y,out _);da.SetData(0,new GH_AreaElement(aligned));da.SetData(1,new Plane(aligned.NodeList[0].Position,x,y));
    }
}
