using Grasshopper.Kernel;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Settings;
using Rhino2Straus.Grasshopper.Datatype;

namespace Rhino2Straus.Grasshopper;

public abstract class NativeSectionComponent : SafeComponent
{
    protected abstract NativeSectionSpec Specification { get; }
    protected NativeSectionComponent(string name,string nickname,string description,string category,string subcategory)
        : base(name,nickname,description,category,subcategory) { }
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Name","Name","Section name",GH_ParamAccess.item,Specification.Name);
        foreach(var dimension in Specification.Parameters)
            p.AddNumberParameter(dimension.Name,dimension.Name,dimension.Description,GH_ParamAccess.item,dimension.Default);
        p.AddGenericParameter("Units","Units","Model units; default kN, m, C",GH_ParamAccess.item); p[p.ParamCount-1].Optional=true;
        p.AddGenericParameter("Options","Options","Native Section Options; default 24 nominal divisions, automatic circular divisions, include shear",GH_ParamAccess.item); p[p.ParamCount-1].Optional=true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Section","Section","Connect to a beam property; native definition survives Build and Merge",GH_ParamAccess.item);
        p.AddBrepParameter("Profile","Profile","Discretised native profile in centroidal XY, in model length units",GH_ParamAccess.list);
        ((IGH_PreviewObject)p[1]).Hidden=true; // Section Goo supplies the same preview; keep the explicit geometry socket available.
    }
    protected override void Solve(IGH_DataAccess da)
    {
        var dimensions=Specification.DefaultDimensions(); int input=1;
        foreach(var dimension in Specification.Parameters) dimensions[dimension.Index]=Item<double>(da,input++);
        GH_Units units=null; da.GetData(input++,ref units);
        NativeSectionOptions options=null; da.GetData(input,ref options);
        var section=NativeSections.Create(Item<string>(da,0),Specification.IsBgl,Specification.Shape,dimensions,units?.Value??new ModelUnitsModel(),options);
        da.SetData(0,new GH_FrameSection(section)); da.SetDataList(1,section.Breps);
    }
}
