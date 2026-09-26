using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Grasshopper.Datatype;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class RigidLinkPropertyComponent : SafeComponent
{
    public RigidLinkPropertyComponent() : base("Rigid Link Property","Rigid Link Property","Rigid body link in the selected global plane.",ComponentTopics.Tab,ComponentTopics.ConstraintsLinks) { }
    public override Guid ComponentGuid => new Guid("5bb78743-9277-50a6-9bb6-a7403a73a347");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Options","Options","Optional Link Constraint Options; default global XYZ",GH_ParamAccess.item);p[0].Optional=true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddGenericParameter("Property","Property","Property",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)
    {

        LinkConstraintOptions options=null;da.GetData(0,ref options);
        var property=new LinkPropertyModel(LinkPropertyModel.LinkPropertyTypes.RIGID) { Options=(options??new()).Copy() };
        property.Options.Validate();
        da.SetData(0,new GH_LinkProperty(property));
    }
}
