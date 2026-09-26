using System;
using Rhino2Straus.Core.ElementProperties;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class NativeSectionBglRhsComponent : NativeSectionComponent
{
    public NativeSectionBglRhsComponent() : base("Beam Section BGL Rectangular Hollow", "BGL Rectangular Hollow", "Native Straus7 R31 section, stiffness and discretised profile preview. Requires an API licence. Default dimensions use metres; connect model Units when different.", "Rhino2Straus", "02-Properties") { }
    public override Guid ComponentGuid => new Guid("d6759ff7-bad4-5ea0-8869-e9212d7f5f05");
    protected override NativeSectionSpec Specification => NativeSectionCatalog.BglRhs;
}
