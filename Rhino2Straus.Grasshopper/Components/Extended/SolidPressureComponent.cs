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

public sealed class SolidPressureComponent : SafeComponent
{
    public SolidPressureComponent() : base("Solid Face Pressure", "Solid Face Pressure", "Solid Face Pressure", "Rhino2Straus", "06-Loads") { }
    public override Guid ComponentGuid => new Guid("14f3a8cb-c2ce-5620-aebc-0f30f777a749");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Solid", "Solid", "Solid", GH_ParamAccess.item);
        p.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
        p.AddIntegerParameter("Face", "Face", "Native Straus7 face number; see API local brick coordinates", GH_ParamAccess.item, 1);
        p.AddNumberParameter("Pressure", "Pressure", "Positive pressure acts into the element", GH_ParamAccess.item, 0.0);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Solid", "Solid", "Solid", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var s=new SolidElementModel(Item<GH_SolidElement>(da,0).Value);int face=Item<int>(da,2);double value=Item<double>(da,3);
        int max=s.Nodes.Count==4?4:s.Nodes.Count==6?5:6;if(face<1||face>max||!double.IsFinite(value))throw new ArgumentException("Invalid face or pressure.");
        s.Pressures.Add(new SolidPressure(Item<GH_LoadCase>(da,1).Value,face,value));da.SetData(0,new GH_SolidElement(s));
    }
}
