using System;
using Rhino2Straus.Core.ElementProperties;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class NativeSectionTrapezoidHollowComponent : NativeSectionComponent
{
    public NativeSectionTrapezoidHollowComponent() : base("Beam Section Hollow Trapezoid", "Hollow Trapezoid", "Native Straus7 R31 section, stiffness and discretised profile preview. Requires an API licence. Default dimensions use metres; connect model Units when different.", "Rhino2Straus", "02-Properties") { }
    public override Guid ComponentGuid => new Guid("64ac1c35-8cc9-5d7d-80f6-e5b98ee3aed1");
    protected override NativeSectionSpec Specification => NativeSectionCatalog.TrapezoidHollow;
}
