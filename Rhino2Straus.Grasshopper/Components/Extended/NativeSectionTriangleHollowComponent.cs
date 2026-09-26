using System;
using Rhino2Straus.Core.ElementProperties;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class NativeSectionTriangleHollowComponent : NativeSectionComponent
{
    public NativeSectionTriangleHollowComponent() : base("Beam Section Hollow Triangle", "Hollow Triangle", "Native Straus7 R31 section, stiffness and discretised profile preview. Requires an API licence. Default dimensions use metres; connect model Units when different.", "Rhino2Straus", "02-Properties") { }
    public override Guid ComponentGuid => new Guid("8e594d6b-8d5b-501f-aa04-f1036f6e6290");
    protected override NativeSectionSpec Specification => NativeSectionCatalog.TriangleHollow;
}
