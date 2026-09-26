using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Grasshopper.Datatype;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class MasterSlaveLinkPropertyComponent : SafeComponent
{
    public MasterSlaveLinkPropertyComponent() : base("MasterSlave Link Property","MasterSlave Link Property","Equal or opposite global degrees of freedom, configured by Link Constraint Options.","Rhino2Straus","02-Properties") { }
    public override Guid ComponentGuid => new Guid("5110325c-9b48-5412-8960-150ea6700ce6");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Options","Options","Optional Link Constraint Options; default global XYZ",GH_ParamAccess.item);p[0].Optional=true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddGenericParameter("Property","Property","Property",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)
    {

        LinkConstraintOptions options=null;da.GetData(0,ref options);
        var property=new LinkPropertyModel(LinkPropertyModel.LinkPropertyTypes.MASTER_SLAVE) { Options=(options??new()).Copy() };
        property.Options.Validate();
        da.SetData(0,new GH_LinkProperty(property));
    }
}
