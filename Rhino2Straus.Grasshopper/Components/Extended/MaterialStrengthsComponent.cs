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

public sealed class MaterialStrengthsComponent : SafeComponent
{
    public MaterialStrengthsComponent() : base("Material Design Strengths", "Material Design Strengths", "Metadata for downstream design checks. Does not define plastic constitutive laws in the Straus7 solver.", "Rhino2Straus", "01-Materials") { }
    public override Guid ComponentGuid => new Guid("fbdbae77-965a-5701-87dc-d68e416113cd");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Material", "Material", "Material", GH_ParamAccess.item);
        p.AddNumberParameter("fy", "fy", "Nominal yield strength in MPa; 0 if not defined", GH_ParamAccess.item, 0.0);
        p.AddNumberParameter("fu", "fu", "Nominal tensile strength in MPa; 0 if not defined", GH_ParamAccess.item, 0.0);
        p.AddNumberParameter("fck", "fck", "Nominal compressive strength in MPa; 0 if not defined", GH_ParamAccess.item, 0.0);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Material", "Material", "Material", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var m=new MaterialModel(Item<Rhino2Straus.Grasshopper.Datatype.GH_Material>(da,0).Value);double fy=Item<double>(da,1),fu=Item<double>(da,2),fck=Item<double>(da,3);
        if(new[]{fy,fu,fck}.Any(x=>!double.IsFinite(x)||x<0)||(fy>0&&fu>0&&fu<fy))throw new ArgumentException("Use finite nonnegative strengths; fu must not be below fy when both are defined.");
        m.TensileYeld=fy;m.TensileStrength=fu;m.CompressiveStrength=fck;da.SetData(0,new Rhino2Straus.Grasshopper.Datatype.GH_Material(m));
    }
}
