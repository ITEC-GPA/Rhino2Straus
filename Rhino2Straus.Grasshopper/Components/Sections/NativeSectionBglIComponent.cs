using System;
using Rhino2Straus.Core.ElementProperties;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class NativeSectionBglIComponent : NativeSectionComponent
{
    public NativeSectionBglIComponent() : base("Beam Section BGL I", "BGL I", "Native Straus7 R31 section, stiffness and discretised profile preview. Requires an API licence. Default dimensions use metres; connect model Units when different.", ComponentTopics.Tab, ComponentTopics.Sections) { }
    public override Guid ComponentGuid => new Guid("f7da393f-3d86-5786-9b60-f2f08194d311");
    protected override NativeSectionSpec Specification => NativeSectionCatalog.BglI;
}
