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

public sealed class BxsSectionComponent : SafeComponent
{
    public BxsSectionComponent() : base("Beam Section BXS", "Beam Section BXS", "Read native stiffness and full profile from a Straus7 BXS file. Requires the API licence. BXS bytes are embedded in the section object for export.", "Rhino2Straus", "02-Properties") { }
    public override Guid ComponentGuid => new Guid("bebd7cde-7e95-5906-8d99-0ccf0485ed67");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("BXS file", "BXS file", "BXS file", GH_ParamAccess.item);
        p.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item, "BXS");
        p.AddGenericParameter("Units", "Units", "Model units; defaults to kN, m, C", GH_ParamAccess.item);p[2].Optional=true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Section", "Section", "Section", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        GH_Units units=null;da.GetData(2,ref units);
        da.SetData(0,new GH_FrameSection(BxsSections.Read(Item<string>(da,0),Item<string>(da,1),units?.Value??new ModelUnitsModel())));
    }
}
