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

public sealed class SectionInsertionComponent : SafeComponent
{
    public SectionInsertionComponent() : base("Beam Section Insertion", "Beam Section Insertion", "Beam Section Insertion", ComponentTopics.Tab, ComponentTopics.AxesOffsets) { }
    public override Guid ComponentGuid => new Guid("8ca1855c-ccad-5a3e-860f-bb6e90bc3290");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Section", "Section", "Section", GH_ParamAccess.item);
        p.AddIntegerParameter("Insertion", "Insertion", "0=LT,1=CT,2=RT,3=LC,4=centroid,5=RC,6=LB,7=CB,8=RB", GH_ParamAccess.item, 4);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Section", "Section", "Section", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var s=new FrameSectionModel(Item<GH_FrameSection>(da,0).Value);int insertion=Item<int>(da,1);if(insertion<0||insertion>8)throw new ArgumentException("Insertion must be between 0 and 8.");s.Offset=(FrameSectionModel.OffsetTypes)insertion;da.SetData(0,new GH_FrameSection(s));
    }
}
