using System;
using Grasshopper.Kernel;
using Rhino2Straus.Core.ElementProperties;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class NativeSectionOptionsComponent : SafeComponent
{
    public NativeSectionOptionsComponent() : base("Native Section Options","Section Options","Discretisation and shear calculation for native Straus7 sections.","Rhino2Straus","02-Properties") { }
    public override Guid ComponentGuid => new Guid("32b4f973-ae10-59e5-9663-bb2f623b873a");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddIntegerParameter("Divisions","Divisions","Nominal divisions along the longest ordinate, 4..200",GH_ParamAccess.item,24);
        p.AddIntegerParameter("Circular","Circular","0=automatic; 1..6=8,16,24,32,40,48 divisions",GH_ParamAccess.item,0);
        p.AddBooleanParameter("Shear","Shear","Calculate shear areas (thick beam formulation)",GH_ParamAccess.item,true);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddGenericParameter("Options","Options","Native section settings",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)
    {
        var options=new NativeSectionOptions { Divisions=Item<int>(da,0),CircularDivisions=Item<int>(da,1),IncludeShear=Item<bool>(da,2) };
        options.Validate(); da.SetData(0,options);
    }
}
