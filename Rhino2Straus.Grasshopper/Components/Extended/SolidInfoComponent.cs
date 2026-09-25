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

public sealed class SolidInfoComponent : SafeComponent
{
    public SolidInfoComponent() : base("Solid Info", "Solid Info", "Solid Info", "Rhino2Straus", "09-Tools") { }
    public override Guid ComponentGuid => new Guid("f2ea193f-a0d6-5827-bbc9-f5846a4744d2");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Solid", "Solid", "Solid", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddIntegerParameter("ID", "ID", "ID", GH_ParamAccess.item);
        p.AddMeshParameter("Geometry", "Geometry", "Geometry", GH_ParamAccess.item);
        p.AddGenericParameter("Material", "Material", "Material", GH_ParamAccess.item);
        p.AddNumberParameter("Volume", "Volume", "Volume", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var s=Item<GH_SolidElement>(da,0).Value;da.SetData(0,s.Id);da.SetData(1,s.ToMesh());da.SetData(2,new Rhino2Straus.Grasshopper.Datatype.GH_Material(s.Material));da.SetData(3,s.SignedVolume());
    }
}
