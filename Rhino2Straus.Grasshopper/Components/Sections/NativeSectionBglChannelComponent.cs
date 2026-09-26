using System;
using Rhino2Straus.Core.ElementProperties;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class NativeSectionBglChannelComponent : NativeSectionComponent
{
    public NativeSectionBglChannelComponent() : base("Beam Section BGL Channel", "BGL Channel", "Native Straus7 R31 section, stiffness and discretised profile preview. Requires an API licence. Default dimensions use metres; connect model Units when different.", ComponentTopics.Tab, ComponentTopics.Sections) { }
    public override Guid ComponentGuid => new Guid("b6801221-ddec-5fc6-94e3-5370138aac62");
    protected override NativeSectionSpec Specification => NativeSectionCatalog.BglChannel;
}
