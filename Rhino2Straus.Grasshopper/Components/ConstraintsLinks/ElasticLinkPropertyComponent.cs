using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Grasshopper.Datatype;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class ElasticLinkPropertyComponent : SafeComponent
{
    public ElasticLinkPropertyComponent() : base("Elastic Link Property","Elastic Link Property","Six independent global spring stiffnesses; exported as a native connection beam.",ComponentTopics.Tab,ComponentTopics.ConstraintsLinks) { }
    public override Guid ComponentGuid => new Guid("7f392dfa-6c41-514b-8bc1-81963ad79528");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddNumberParameter("Kx","Kx","Nonnegative stiffness in model force/length units, global XYZ. Sample default; replace with actual connection data.",GH_ParamAccess.item,1000000);
        p.AddNumberParameter("Ky","Ky","Nonnegative stiffness in model force/length units, global XYZ. Sample default; replace with actual connection data.",GH_ParamAccess.item,1000000);
        p.AddNumberParameter("Kz","Kz","Nonnegative stiffness in model force/length units, global XYZ. Sample default; replace with actual connection data.",GH_ParamAccess.item,1000000);
        p.AddNumberParameter("Rx","Rx","Nonnegative stiffness in model moment/radian units, global XYZ. Sample default; replace with actual connection data.",GH_ParamAccess.item,1000);
        p.AddNumberParameter("Ry","Ry","Nonnegative stiffness in model moment/radian units, global XYZ. Sample default; replace with actual connection data.",GH_ParamAccess.item,1000);
        p.AddNumberParameter("Rz","Rz","Nonnegative stiffness in model moment/radian units, global XYZ. Sample default; replace with actual connection data.",GH_ParamAccess.item,1000);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p) => p.AddGenericParameter("Property","Property","Property",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)
    {

        var k=new double[6];for(int i=0;i<6;i++){k[i]=Item<double>(da,i);if(!double.IsFinite(k[i])||k[i]<0)throw new ArgumentException("Stiffness must be finite and nonnegative.");}
        da.SetData(0,new GH_LinkProperty(new LinkPropertyModel(LinkPropertyModel.LinkPropertyTypes.GEN,k[0],k[1],k[2],k[3],k[4],k[5])));
    }
}
