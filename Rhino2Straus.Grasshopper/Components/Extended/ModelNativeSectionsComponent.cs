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

public sealed class ModelNativeSectionsComponent : SafeComponent
{
    public ModelNativeSectionsComponent() : base("Model Native Beam Sections", "Model Native Beam Sections", "Actual section stiffness values read from Straus7 and stored inside Model.Results. Geometry units are the Model units.", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("7f3518af-a64c-5b6a-a415-76c132b64168");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Sections", "Sections", "Sections", GH_ParamAccess.list);
        p.AddIntegerParameter("Property IDs", "Property IDs", "Property IDs", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var m=Item<GH_Model>(da,0).Value;var r=m.Results??throw new InvalidOperationException("Model has no results.");r.ValidateFor(m);
        da.SetDataList(0,r.BeamSections.Select(x=>new GH_ObjectWrapper(x)));da.SetDataList(1,r.BeamSections.Select(x=>x.PropertyId));
    }
}
