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

public sealed class BeamAssignPropertyComponent : SafeComponent
{
    public BeamAssignPropertyComponent() : base("Assign Beam Property", "Assign Beam Property", "Assign Beam Property", ComponentTopics.Tab, ComponentTopics.Properties) { }
    public override Guid ComponentGuid => new Guid("0386c1ed-b47b-5e21-87b0-1ed59b3cd1d7");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
        p.AddGenericParameter("Property", "Property", "Property", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Beam", "Beam", "Beam", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var b=new FrameElementModel(Item<GH_FrameElement>(da,0).Value);b.FrameProperty=Item<GH_FrameProperty>(da,1).Value;b.Material=b.FrameProperty.Material;b.FrameSection=b.FrameProperty.Section;b.Breps=new();da.SetData(0,new GH_FrameElement(b));
    }
}
