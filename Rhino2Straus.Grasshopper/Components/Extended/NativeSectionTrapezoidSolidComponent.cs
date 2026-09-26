using System;
using Rhino2Straus.Core.ElementProperties;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class NativeSectionTrapezoidSolidComponent : NativeSectionComponent
{
    public NativeSectionTrapezoidSolidComponent() : base("Beam Section Solid Trapezoid", "Solid Trapezoid", "Native Straus7 R31 section, stiffness and discretised profile preview. Requires an API licence. Default dimensions use metres; connect model Units when different.", "Rhino2Straus", "02-Properties") { }
    public override Guid ComponentGuid => new Guid("e2e9a83d-cea5-530f-b61c-42e74e3c3786");
    protected override NativeSectionSpec Specification => NativeSectionCatalog.TrapezoidSolid;
}
