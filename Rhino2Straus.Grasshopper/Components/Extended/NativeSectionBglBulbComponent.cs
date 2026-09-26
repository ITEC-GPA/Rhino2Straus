using System;
using Rhino2Straus.Core.ElementProperties;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class NativeSectionBglBulbComponent : NativeSectionComponent
{
    public NativeSectionBglBulbComponent() : base("Beam Section BGL Bulb Flat", "BGL Bulb Flat", "Native Straus7 R31 section, stiffness and discretised profile preview. Requires an API licence. Default dimensions use metres; connect model Units when different.", "Rhino2Straus", "02-Properties") { }
    public override Guid ComponentGuid => new Guid("e117d60a-bbe3-51bd-82f0-b85ce92e03e2");
    protected override NativeSectionSpec Specification => NativeSectionCatalog.BglBulb;
}
