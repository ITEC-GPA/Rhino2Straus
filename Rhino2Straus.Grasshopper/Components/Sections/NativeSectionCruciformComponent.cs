using System;
using Rhino2Straus.Core.ElementProperties;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class NativeSectionCruciformComponent : NativeSectionComponent
{
    public NativeSectionCruciformComponent() : base("Beam Section Cruciform", "Cruciform", "Native Straus7 R31 section, stiffness and discretised profile preview. Requires an API licence. Default dimensions use metres; connect model Units when different.", ComponentTopics.Tab, ComponentTopics.Sections) { }
    public override Guid ComponentGuid => new Guid("230c9c08-9528-5192-95f6-cd59b8e99739");
    protected override NativeSectionSpec Specification => NativeSectionCatalog.Cruciform;
}
