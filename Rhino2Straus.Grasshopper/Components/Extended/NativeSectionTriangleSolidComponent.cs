using System;
using Rhino2Straus.Core.ElementProperties;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class NativeSectionTriangleSolidComponent : NativeSectionComponent
{
    public NativeSectionTriangleSolidComponent() : base("Beam Section Solid Triangle", "Solid Triangle", "Native Straus7 R31 section, stiffness and discretised profile preview. Requires an API licence. Default dimensions use metres; connect model Units when different.", "Rhino2Straus", "02-Properties") { }
    public override Guid ComponentGuid => new Guid("c4db48f8-5cb3-550a-a5cf-baccd9a4dcff");
    protected override NativeSectionSpec Specification => NativeSectionCatalog.TriangleSolid;
}
