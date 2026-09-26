using System;
using Rhino2Straus.Core.ElementProperties;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class NativeSectionTopHatComponent : NativeSectionComponent
{
    public NativeSectionTopHatComponent() : base("Beam Section Top Hat Channel", "Top Hat Channel", "Native Straus7 R31 section, stiffness and discretised profile preview. Requires an API licence. Default dimensions use metres; connect model Units when different.", ComponentTopics.Tab, ComponentTopics.Sections) { }
    public override Guid ComponentGuid => new Guid("15cc7e31-05d2-5bae-8fa6-720715a8e414");
    protected override NativeSectionSpec Specification => NativeSectionCatalog.TopHat;
}
