using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Grasshopper.Datatype;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class SectorSymmetryLinkPropertyComponent : SafeComponent
{
    public SectorSymmetryLinkPropertyComponent() : base("SectorSymmetry Link Property","SectorSymmetry Link Property","Native sector-symmetry link about global X, Y or Z.",ComponentTopics.Tab,ComponentTopics.ConstraintsLinks) { }
    public override Guid ComponentGuid => new Guid("a475b0d6-9df7-59fd-a41b-04dc2645be93");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Options","Options","Optional Link Constraint Options; default global XYZ",GH_ParamAccess.item);p[0].Optional=true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddGenericParameter("Property","Property","Property",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)
    {

        LinkConstraintOptions options=null;da.GetData(0,ref options);
        var property=new LinkPropertyModel(LinkPropertyModel.LinkPropertyTypes.SECTOR_SYMMETRY) { Options=(options??new()).Copy() };
        property.Options.Validate();
        da.SetData(0,new GH_LinkProperty(property));
    }
}
