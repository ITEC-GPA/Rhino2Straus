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

public sealed class SelfWeightComponent : SafeComponent
{
    public SelfWeightComponent() : base("Self Weight", "Self Weight", "Self Weight", ComponentTopics.Tab, ComponentTopics.Loads) { }
    public override Guid ComponentGuid => new Guid("183c07fe-c460-5163-a09b-df12543e3037");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
        p.AddGenericParameter("Units", "Units", "Optional units; default kN, m, kJ, C. Connect the same Units used by the Model when different.", GH_ParamAccess.item);p[1].Optional=true;
        p.AddIntegerParameter("Axis", "Axis", "0=X, 1=Y, 2=Z", GH_ParamAccess.item, 2);
        p.AddNumberParameter("Factor", "Factor", "Signed acceleration multiplier; -1 is downward on Z", GH_ParamAccess.item, -1.0);
        p.AddBooleanParameter("Structural mass", "Structural mass", "Structural mass", GH_ParamAccess.item, true);
        p.AddBooleanParameter("Nonstructural mass", "Nonstructural mass", "Nonstructural mass", GH_ParamAccess.item, true);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Self weight", "Self weight", "Self weight", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var c = Item<GH_LoadCase>(da,0).Value;var units=new GH_Units(new ModelUnitsModel());da.GetData(1,ref units);var u=units.Value;
        int axis = Item<int>(da,2); double factor = Item<double>(da,3);
        if (axis < 0 || axis > 2 || !double.IsFinite(factor)) throw new ArgumentException("Invalid gravity axis or factor.");
        da.SetData(0,new GH_SelfWeight(new SelfWeightModel { LoadCase = c,
            GlobalInertiaLoad = SelfWeightModel.GlobalInertiaLoads.Gravity,
            GravityDirection = (SelfWeightModel.GravityDirections)axis,
            GravityValue = 9.80665 * StrausUnits.LengthPerMetre(u.LengthUnit) * factor,
            StructuralMassAcceleration = Item<bool>(da,4), NonStructuralMassAcceleration = Item<bool>(da,5) }));
    }
}
