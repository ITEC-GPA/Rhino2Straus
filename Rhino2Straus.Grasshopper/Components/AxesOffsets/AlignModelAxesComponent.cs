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

public sealed class AlignModelAxesComponent : SafeComponent
{
    public AlignModelAxesComponent() : base("Align Model Local Axes", "Align Model Local Axes", "Align beam principal axes and/or plate local axes. Parallel reference directions produce an error; results are cleared.", ComponentTopics.Tab, ComponentTopics.AxesOffsets) { }
    public override Guid ComponentGuid => new Guid("306b1f1f-3675-588d-9dcb-7095fa5859b9");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        p.AddVectorParameter("Direction", "Direction", "Direction", GH_ParamAccess.item, Vector3d.XAxis);
        p.AddBooleanParameter("Beams", "Beams", "Beams", GH_ParamAccess.item, true);
        p.AddBooleanParameter("Plates", "Plates", "Plates", GH_ParamAccess.item, false);
        p.AddIntegerParameter("Axis", "Axis", "Axis", GH_ParamAccess.item, 1);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        da.SetData(0,new GH_Model(ElementAlignment.Model(Item<GH_Model>(da,0).Value,Item<Vector3d>(da,1),Item<bool>(da,2),Item<bool>(da,3),Item<int>(da,4))));
    }
}
