using System;
using Rhino2Straus.Core.ElementProperties;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class NativeSectionTeeLippedComponent : NativeSectionComponent
{
    public NativeSectionTeeLippedComponent() : base("Beam Section Lipped Tee", "Lipped Tee", "Native Straus7 R31 section, stiffness and discretised profile preview. Requires an API licence. Default dimensions use metres; connect model Units when different.", "Rhino2Straus", "02-Properties") { }
    public override Guid ComponentGuid => new Guid("1763f0dd-5511-5577-ae38-3c589bf1da8d");
    protected override NativeSectionSpec Specification => NativeSectionCatalog.TeeLipped;
}
