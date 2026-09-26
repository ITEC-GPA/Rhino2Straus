using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Grasshopper.Datatype;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class PinnedLinkPropertyComponent : SafeComponent
{
    public PinnedLinkPropertyComponent() : base("Pinned Link Property","Pinned Link Property","Native Straus7 pinned link between two nodes.","Rhino2Straus","02-Properties") { }
    public override Guid ComponentGuid => new Guid("5330401b-325b-562d-8883-54128f1c7759");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Options","Options","Optional Link Constraint Options; default global XYZ",GH_ParamAccess.item);p[0].Optional=true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddGenericParameter("Property","Property","Property",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)
    {

        LinkConstraintOptions options=null;da.GetData(0,ref options);
        var property=new LinkPropertyModel(LinkPropertyModel.LinkPropertyTypes.PINNED) { Options=(options??new()).Copy() };
        property.Options.Validate();
        da.SetData(0,new GH_LinkProperty(property));
    }
}
