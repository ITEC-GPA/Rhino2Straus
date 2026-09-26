using System;
using Rhino2Straus.Core.ElementProperties;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class NativeSectionBglTeeComponent : NativeSectionComponent
{
    public NativeSectionBglTeeComponent() : base("Beam Section BGL Tee", "BGL Tee", "Native Straus7 R31 section, stiffness and discretised profile preview. Requires an API licence. Default dimensions use metres; connect model Units when different.", "Rhino2Straus", "02-Properties") { }
    public override Guid ComponentGuid => new Guid("6eac3294-a943-5b5c-ac4f-3fa30a6ec4c3");
    protected override NativeSectionSpec Specification => NativeSectionCatalog.BglTee;
}
