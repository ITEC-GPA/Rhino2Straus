using System;
using Rhino2Straus.Core.ElementProperties;
namespace Rhino2Straus.Grasshopper.Components.Extended;
public sealed class NativeSectionBglAngleComponent : NativeSectionComponent
{
    public NativeSectionBglAngleComponent() : base("Beam Section BGL Angle", "BGL Angle", "Native Straus7 R31 section, stiffness and discretised profile preview. Requires an API licence. Default dimensions use metres; connect model Units when different.", "Rhino2Straus", "02-Properties") { }
    public override Guid ComponentGuid => new Guid("455515b3-0e31-5e4f-a1dd-a27c3f8fa04e");
    protected override NativeSectionSpec Specification => NativeSectionCatalog.BglAngle;
}
