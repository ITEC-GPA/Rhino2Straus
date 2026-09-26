using System;
using Rhino2Straus.Core.ElementProperties;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class NativeSectionLipChannelComponent : NativeSectionComponent
{
    public NativeSectionLipChannelComponent() : base("Beam Section Lipped Channel", "Lipped Channel", "Native Straus7 R31 section, stiffness and discretised profile preview. Requires an API licence. Default dimensions use metres; connect model Units when different.", "Rhino2Straus", "02-Properties") { }
    public override Guid ComponentGuid => new Guid("958d7da9-62f4-5092-bdf9-d13830e7e8de");
    protected override NativeSectionSpec Specification => NativeSectionCatalog.LipChannel;
}
