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

public sealed class ValidateModelComponent : SafeComponent
{
    public ValidateModelComponent() : base("Validate Straus Model", "Validate Straus Model", "Checks model data and supported Straus7 features before export; does not check structural adequacy", "Rhino2Straus", "07-Model") { }
    public override Guid ComponentGuid => new Guid("595409be-4738-594a-b57d-39bc2437d0a6");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddBooleanParameter("Valid", "Valid", "Valid", GH_ParamAccess.item);
        p.AddTextParameter("Errors", "Errors", "Errors", GH_ParamAccess.list);
        p.AddTextParameter("Warnings", "Warnings", "Warnings", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var v=ModelValidator.Validate(Item<GH_Model>(da,0).Value); da.SetData(0,v.IsValid); da.SetDataList(1,v.Errors); da.SetDataList(2,v.Warnings); Message=v.IsValid?"Valid":"Invalid";
    }
}
