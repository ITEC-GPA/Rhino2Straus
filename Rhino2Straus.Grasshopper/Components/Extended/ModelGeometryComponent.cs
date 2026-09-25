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

public sealed class ModelGeometryComponent : SafeComponent
{
    public ModelGeometryComponent() : base("Model Physical Geometry", "Model Physical Geometry", "Model Physical Geometry", "Rhino2Straus", "13-Preview") { }
    public override Guid ComponentGuid => new Guid("e42dd5d1-840b-5ce6-96dd-49ba371fdc07");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddBrepParameter("Beam solids", "Beam solids", "Beam solids", GH_ParamAccess.list);
        p.AddMeshParameter("Plate solids", "Plate solids", "Plate solids", GH_ParamAccess.list);
        p.AddLineParameter("Centerlines", "Centerlines", "Centerlines", GH_ParamAccess.list);
        p.AddMeshParameter("Solids","Solids","Solid element skins",GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var p=new ModelPreview(Item<GH_Model>(da,0).Value);da.SetDataList(0,p.Beams);da.SetDataList(1,p.Plates);da.SetDataList(2,p.Lines);da.SetDataList(3,p.Solids);
    }
}
