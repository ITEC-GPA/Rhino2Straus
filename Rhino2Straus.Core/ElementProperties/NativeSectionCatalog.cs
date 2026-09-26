using System;
using System.Collections.Generic;
using St7API;
namespace Rhino2Straus.Core.ElementProperties;

public sealed record NativeSectionParameter(string Name, int Index, double Default, string Description);
public sealed record NativeSectionSpec(string Name, int Shape, bool IsBgl, NativeSectionParameter[] Parameters)
{
    public double[] DefaultDimensions() { var a=new double[IsBgl?16:6]; foreach(var p in Parameters) a[p.Index]=p.Default; return a; }
}
public static class NativeSectionCatalog
{
    public static readonly NativeSectionSpec LipChannel = new("Lipped Channel",St7.bsLipChannel,false,new NativeSectionParameter[] {
        new("B",0,0.15,"Straus7 B in selected model length units; native section diagram in R3 API manual"),
        new("D",1,0.3,"Straus7 D in selected model length units; native section diagram in R3 API manual"),
        new("L",2,0.025,"Straus7 L in selected model length units; native section diagram in R3 API manual"),
        new("T1",3,0.008,"Straus7 T1 in selected model length units; native section diagram in R3 API manual"),
        new("T2",4,0.006,"Straus7 T2 in selected model length units; native section diagram in R3 API manual"),
        new("T3",5,0.006,"Straus7 T3 in selected model length units; native section diagram in R3 API manual"),
    });
    public static readonly NativeSectionSpec TopHat = new("Top Hat Channel",St7.bsTopHatChannel,false,new NativeSectionParameter[] {
        new("B",0,0.15,"Straus7 B in selected model length units; native section diagram in R3 API manual"),
        new("D",1,0.3,"Straus7 D in selected model length units; native section diagram in R3 API manual"),
        new("L",2,0.025,"Straus7 L in selected model length units; native section diagram in R3 API manual"),
        new("T1",3,0.008,"Straus7 T1 in selected model length units; native section diagram in R3 API manual"),
        new("T2",4,0.006,"Straus7 T2 in selected model length units; native section diagram in R3 API manual"),
        new("T3",5,0.006,"Straus7 T3 in selected model length units; native section diagram in R3 API manual"),
    });
    public static readonly NativeSectionSpec Z = new("Z",St7.bsZSection,false,new NativeSectionParameter[] {
        new("B1",0,0.15,"Straus7 B1 in selected model length units; native section diagram in R3 API manual"),
        new("B2",1,0.15,"Straus7 B2 in selected model length units; native section diagram in R3 API manual"),
        new("D",2,0.3,"Straus7 D in selected model length units; native section diagram in R3 API manual"),
        new("T1",3,0.01,"Straus7 T1 in selected model length units; native section diagram in R3 API manual"),
        new("T2",4,0.01,"Straus7 T2 in selected model length units; native section diagram in R3 API manual"),
        new("T3",5,0.008,"Straus7 T3 in selected model length units; native section diagram in R3 API manual"),
    });
    public static readonly NativeSectionSpec TrapezoidSolid = new("Solid Trapezoid",St7.bsTrapezoidSolid,false,new NativeSectionParameter[] {
        new("B1",0,0.2,"Straus7 B1 in selected model length units; native section diagram in R3 API manual"),
        new("B2",1,0.3,"Straus7 B2 in selected model length units; native section diagram in R3 API manual"),
        new("D",2,0.3,"Straus7 D in selected model length units; native section diagram in R3 API manual"),
    });
    public static readonly NativeSectionSpec TrapezoidHollow = new("Hollow Trapezoid",St7.bsTrapezoidHollow,false,new NativeSectionParameter[] {
        new("B1",0,0.2,"Straus7 B1 in selected model length units; native section diagram in R3 API manual"),
        new("B2",1,0.3,"Straus7 B2 in selected model length units; native section diagram in R3 API manual"),
        new("D",2,0.3,"Straus7 D in selected model length units; native section diagram in R3 API manual"),
        new("T1",3,0.01,"Straus7 T1 in selected model length units; native section diagram in R3 API manual"),
        new("T2",4,0.01,"Straus7 T2 in selected model length units; native section diagram in R3 API manual"),
    });
    public static readonly NativeSectionSpec TriangleSolid = new("Solid Triangle",St7.bsTriangleSolid,false,new NativeSectionParameter[] {
        new("B",0,0.2,"Straus7 B in selected model length units; native section diagram in R3 API manual"),
        new("D",1,0.3,"Straus7 D in selected model length units; native section diagram in R3 API manual"),
    });
    public static readonly NativeSectionSpec TriangleHollow = new("Hollow Triangle",St7.bsTriangleHollow,false,new NativeSectionParameter[] {
        new("B",0,0.2,"Straus7 B in selected model length units; native section diagram in R3 API manual"),
        new("D",1,0.3,"Straus7 D in selected model length units; native section diagram in R3 API manual"),
        new("T1",3,0.01,"Straus7 T1 in selected model length units; native section diagram in R3 API manual"),
        new("T2",4,0.01,"Straus7 T2 in selected model length units; native section diagram in R3 API manual"),
    });
    public static readonly NativeSectionSpec Cruciform = new("Cruciform",St7.bsCruciform,false,new NativeSectionParameter[] {
        new("B",0,0.2,"Straus7 B in selected model length units; native section diagram in R3 API manual"),
        new("D",1,0.3,"Straus7 D in selected model length units; native section diagram in R3 API manual"),
        new("T1",3,0.01,"Straus7 T1 in selected model length units; native section diagram in R3 API manual"),
        new("T2",4,0.01,"Straus7 T2 in selected model length units; native section diagram in R3 API manual"),
    });
    public static readonly NativeSectionSpec TeeLipped = new("Lipped Tee",St7.bsTSection,false,new NativeSectionParameter[] {
        new("B",0,0.2,"Straus7 B in selected model length units; native section diagram in R3 API manual"),
        new("D",1,0.3,"Straus7 D in selected model length units; native section diagram in R3 API manual"),
        new("L",2,0.03,"Straus7 L in selected model length units; native section diagram in R3 API manual"),
        new("T1",3,0.01,"Straus7 T1 in selected model length units; native section diagram in R3 API manual"),
        new("T2",4,0.008,"Straus7 T2 in selected model length units; native section diagram in R3 API manual"),
        new("T3",5,0.006,"Straus7 T3 in selected model length units; native section diagram in R3 API manual"),
    });
    public static readonly NativeSectionSpec BglI = new("BGL I",St7.bgISection,true,new NativeSectionParameter[] {
        new("D",0,0.3,"Straus7 D in selected model length units; native section diagram in R3 API manual"),
        new("B1",1,0.15,"Straus7 B1 in selected model length units; native section diagram in R3 API manual"),
        new("B2",2,0.15,"Straus7 B2 in selected model length units; native section diagram in R3 API manual"),
        new("tw",3,0.008,"Straus7 tw in selected model length units; native section diagram in R3 API manual"),
        new("tf1",4,0.012,"Straus7 tf1 in selected model length units; native section diagram in R3 API manual"),
        new("tf2",5,0.012,"Straus7 tf2 in selected model length units; native section diagram in R3 API manual"),
        new("Rr1",6,0.008,"Straus7 Rr1 in selected model length units; native section diagram in R3 API manual"),
        new("Rr2",7,0.008,"Straus7 Rr2 in selected model length units; native section diagram in R3 API manual"),
        new("Rt1",8,0,"Straus7 Rt1 in selected model length units; native section diagram in R3 API manual"),
        new("Rt2",9,0,"Straus7 Rt2 in selected model length units; native section diagram in R3 API manual"),
        new("Rt3",10,0,"Straus7 Rt3 in selected model length units; native section diagram in R3 API manual"),
        new("Rt4",11,0,"Straus7 Rt4 in selected model length units; native section diagram in R3 API manual"),
        new("alpha1",12,0,"BGL alpha1 in degrees"),
        new("alpha2",13,0,"BGL alpha2 in degrees"),
    });
    public static readonly NativeSectionSpec BglTee = new("BGL Tee",St7.bgTSection,true,new NativeSectionParameter[] {
        new("D",0,0.3,"Straus7 D in selected model length units; native section diagram in R3 API manual"),
        new("B",1,0.15,"Straus7 B in selected model length units; native section diagram in R3 API manual"),
        new("tw",2,0.008,"Straus7 tw in selected model length units; native section diagram in R3 API manual"),
        new("tf",3,0.012,"Straus7 tf in selected model length units; native section diagram in R3 API manual"),
        new("Rr",4,0.008,"Straus7 Rr in selected model length units; native section diagram in R3 API manual"),
        new("Rt1",5,0,"Straus7 Rt1 in selected model length units; native section diagram in R3 API manual"),
        new("Rt2",6,0,"Straus7 Rt2 in selected model length units; native section diagram in R3 API manual"),
        new("Rt3",7,0,"Straus7 Rt3 in selected model length units; native section diagram in R3 API manual"),
        new("alpha1",8,0,"BGL alpha1 in degrees"),
        new("alpha2",9,0,"BGL alpha2 in degrees"),
    });
    public static readonly NativeSectionSpec BglChannel = new("BGL Channel",St7.bgChannel,true,new NativeSectionParameter[] {
        new("D",0,0.3,"Straus7 D in selected model length units; native section diagram in R3 API manual"),
        new("B1",1,0.15,"Straus7 B1 in selected model length units; native section diagram in R3 API manual"),
        new("B2",2,0.15,"Straus7 B2 in selected model length units; native section diagram in R3 API manual"),
        new("tw",3,0.008,"Straus7 tw in selected model length units; native section diagram in R3 API manual"),
        new("tf1",4,0.012,"Straus7 tf1 in selected model length units; native section diagram in R3 API manual"),
        new("tf2",5,0.012,"Straus7 tf2 in selected model length units; native section diagram in R3 API manual"),
        new("Rr1",6,0.008,"Straus7 Rr1 in selected model length units; native section diagram in R3 API manual"),
        new("Rr2",7,0.008,"Straus7 Rr2 in selected model length units; native section diagram in R3 API manual"),
        new("Rh1",8,0,"Straus7 Rh1 in selected model length units; native section diagram in R3 API manual"),
        new("Rh2",9,0,"Straus7 Rh2 in selected model length units; native section diagram in R3 API manual"),
        new("Rt1",10,0,"Straus7 Rt1 in selected model length units; native section diagram in R3 API manual"),
        new("Rt2",11,0,"Straus7 Rt2 in selected model length units; native section diagram in R3 API manual"),
        new("Rt3",12,0,"Straus7 Rt3 in selected model length units; native section diagram in R3 API manual"),
        new("Rt4",13,0,"Straus7 Rt4 in selected model length units; native section diagram in R3 API manual"),
        new("alpha1",14,0,"BGL alpha1 in degrees"),
        new("alpha2",15,0,"BGL alpha2 in degrees"),
    });
    public static readonly NativeSectionSpec BglAngle = new("BGL Angle",St7.bgAngle,true,new NativeSectionParameter[] {
        new("D",0,0.3,"Straus7 D in selected model length units; native section diagram in R3 API manual"),
        new("B",1,0.15,"Straus7 B in selected model length units; native section diagram in R3 API manual"),
        new("tw",2,0.008,"Straus7 tw in selected model length units; native section diagram in R3 API manual"),
        new("tf",3,0.012,"Straus7 tf in selected model length units; native section diagram in R3 API manual"),
        new("Rr",4,0.008,"Straus7 Rr in selected model length units; native section diagram in R3 API manual"),
        new("Rh",5,0,"Straus7 Rh in selected model length units; native section diagram in R3 API manual"),
        new("Rt1",6,0,"Straus7 Rt1 in selected model length units; native section diagram in R3 API manual"),
        new("Rt2",7,0,"Straus7 Rt2 in selected model length units; native section diagram in R3 API manual"),
        new("Rt3",8,0,"Straus7 Rt3 in selected model length units; native section diagram in R3 API manual"),
        new("Rt4",9,0,"Straus7 Rt4 in selected model length units; native section diagram in R3 API manual"),
        new("alpha1",10,0,"BGL alpha1 in degrees"),
        new("alpha2",11,0,"BGL alpha2 in degrees"),
    });
    public static readonly NativeSectionSpec BglRhs = new("BGL Rectangular Hollow",St7.bgRectangularHollow,true,new NativeSectionParameter[] {
        new("D",0,0.3,"Straus7 D in selected model length units; native section diagram in R3 API manual"),
        new("B",1,0.15,"Straus7 B in selected model length units; native section diagram in R3 API manual"),
        new("tw",2,0.008,"Straus7 tw in selected model length units; native section diagram in R3 API manual"),
        new("tf",3,0.012,"Straus7 tf in selected model length units; native section diagram in R3 API manual"),
        new("Ri",4,0.008,"Straus7 Ri in selected model length units; native section diagram in R3 API manual"),
        new("Ro",5,0.016,"Straus7 Ro in selected model length units; native section diagram in R3 API manual"),
    });
    public static readonly NativeSectionSpec BglBulb = new("BGL Bulb Flat",St7.bgBulbFlat,true,new NativeSectionParameter[] {
        new("D",0,0.3,"Straus7 D in selected model length units; native section diagram in R3 API manual"),
        new("B",1,0.04,"Straus7 B in selected model length units; native section diagram in R3 API manual"),
        new("t",2,0.01,"Straus7 t in selected model length units; native section diagram in R3 API manual"),
        new("Rr",3,0.008,"Straus7 Rr in selected model length units; native section diagram in R3 API manual"),
        new("Rh",4,0,"Straus7 Rh in selected model length units; native section diagram in R3 API manual"),
        new("Rt1",5,0,"Straus7 Rt1 in selected model length units; native section diagram in R3 API manual"),
        new("Rt2",6,0,"Straus7 Rt2 in selected model length units; native section diagram in R3 API manual"),
        new("Rt3",7,0,"Straus7 Rt3 in selected model length units; native section diagram in R3 API manual"),
        new("alpha",8,30,"BGL alpha in degrees"),
    });
    public static IReadOnlyList<NativeSectionSpec> All { get; } = new[] { LipChannel,TopHat,Z,TrapezoidSolid,TrapezoidHollow,TriangleSolid,TriangleHollow,Cruciform,TeeLipped,BglI,BglTee,BglChannel,BglAngle,BglRhs,BglBulb };
}
