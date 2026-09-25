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

public sealed class SectionInfoComponent : SafeComponent
{
    public SectionInfoComponent() : base("Deconstruct Beam Section", "Deconstruct Beam Section", "Deconstruct Beam Section", "Rhino2Straus", "09-Tools") { }
    public override Guid ComponentGuid => new Guid("23a8b06f-3908-5f71-8fd7-06a6d8833adb");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Section", "Section", "Section", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
        p.AddNumberParameter("Area", "Area", "Area", GH_ParamAccess.item);
        p.AddNumberParameter("I11", "I11", "I11", GH_ParamAccess.item);
        p.AddNumberParameter("I22", "I22", "I22", GH_ParamAccess.item);
        p.AddNumberParameter("J", "J", "J", GH_ParamAccess.item);
        p.AddNumberParameter("Shear A1", "Shear A1", "Shear A1", GH_ParamAccess.item);
        p.AddNumberParameter("Shear A2", "Shear A2", "Shear A2", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var s=Item<GH_FrameSection>(da,0).Value; if(s.SectionGeometryType!=FrameSectionModel.SectionGeometryTypes.Generic)AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,"J and shear areas are calculated by Straus7 during export. Zero here means not yet calculated; a BXS section includes native stiffness values."); da.SetData(0,s.Name); da.SetData(1,s.SectionArea); da.SetData(2,s.I11); da.SetData(3,s.I22); da.SetData(4,s.J); da.SetData(5,s.ShearA1); da.SetData(6,s.ShearA2);
    }
}
