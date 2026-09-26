using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Grasshopper.Datatype;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class LinkConstraintOptionsComponent : SafeComponent
{
    public LinkConstraintOptionsComponent() : base("Link Constraint Options","Link Constraint Options","Options for rigid, master-slave and sector-symmetry links. Global XYZ coordinates.","Rhino2Straus","02-Properties") { }
    public override Guid ComponentGuid => new Guid("b609d6ce-7865-5077-bff4-f3fb8ad39478");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {

        p.AddIntegerParameter("Rigid plane","Plane","0=XYZ, 1=XY, 2=YZ, 3=ZX; used by rigid links",GH_ParamAccess.item,0);
        p.AddIntegerParameter("Symmetry axis","Axis","1=X, 2=Y, 3=Z; used by sector-symmetry links",GH_ParamAccess.item,3);
        foreach(string name in new[]{"UX","UY","UZ","RX","RY","RZ"})
        {
            p.AddIntegerParameter(name,name,"Master-slave: 0=free, 1=equal, -1=opposite",GH_ParamAccess.item,1);
            var value=(Param_Integer)p[p.ParamCount-1];value.AddNamedValue("Free",0);value.AddNamedValue("Equal",1);value.AddNamedValue("Opposite",-1);
        }
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddGenericParameter("Options","Options","Options",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)
    {

        var options=new LinkConstraintOptions{RigidPlane=Item<int>(da,0),SymmetryAxis=Item<int>(da,1)};
        for(int i=0;i<6;i++)options.Dofs[i]=Item<int>(da,i+2);
        options.Validate();da.SetData(0,options);
    }
}
