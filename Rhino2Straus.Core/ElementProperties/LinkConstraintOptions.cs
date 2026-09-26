using System;
using System.Linq;
namespace Rhino2Straus.Core.ElementProperties;

public sealed class LinkConstraintOptions
{
    public int RigidPlane { get; set; }
    public int SymmetryAxis { get; set; } = 3;
    public int[] Dofs { get; set; } = new[]{1,1,1,1,1,1};
    public LinkConstraintOptions Copy() => new() { RigidPlane=RigidPlane, SymmetryAxis=SymmetryAxis, Dofs=(int[])Dofs.Clone() };
    public void Validate()
    {
        if(RigidPlane<0 || RigidPlane>3) throw new ArgumentException("Rigid plane: 0=XYZ, 1=XY, 2=YZ, 3=ZX.");
        if(SymmetryAxis<1 || SymmetryAxis>3) throw new ArgumentException("Symmetry axis: 1=X, 2=Y, 3=Z.");
        if(Dofs==null || Dofs.Length!=6 || Dofs.Any(d=>d is < -1 or > 1)) throw new ArgumentException("Master-slave needs six DoFs: 0=free, 1=equal, -1=opposite.");
    }
}
