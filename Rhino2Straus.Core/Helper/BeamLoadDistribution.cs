using System;
using System.Collections.Generic;
using Rhino2Straus.Core.Loads;

namespace Rhino2Straus.Core.Helper;

public static class BeamLoadDistribution
{
    /// <summary>Model A/B are positions from node 1. Native b is an end distance except dlTrapezoidal; see R3 manual p.1010.</summary>
    public static double[] NativeValues(FrameLoadStrausModel load) => new[] {load.PA,load.PB,load.P1,load.P2,load.A,
        load.LoadSchema==FrameLoadStrausModel.LoadSchemas.TriangularEnds_Keystone ? load.B : 1-load.B};

    public static IReadOnlyList<(double Position,double Value)> Knots(FrameLoadStrausModel load)
    {
        double a=load.A,b=load.B;
        return load.LoadSchema switch
        {
            FrameLoadStrausModel.LoadSchemas.Uniform=>new[]{(a,load.PA),(b,load.PA)},
            FrameLoadStrausModel.LoadSchemas.Keystone=>new[]{(a,load.PA),(b,load.PB)},
            FrameLoadStrausModel.LoadSchemas.Triangular=>new[]{(0d,load.P1),(a,load.PA),(1d,load.P2)},
            FrameLoadStrausModel.LoadSchemas.TriangularEnd1_Keystone=>new[]{(0d,load.P1),(a,load.PA),(b,load.PB)},
            FrameLoadStrausModel.LoadSchemas.Keystone_TriangularEnd2=>new[]{(a,load.PA),(b,load.PB),(1d,load.P2)},
            FrameLoadStrausModel.LoadSchemas.TriangularEnds_Keystone=>new[]{(0d,load.P1),(a,load.PA),(b,load.PB),(1d,load.P2)},
            _=>throw new ArgumentException("Unknown beam load distribution.")
        };
    }
}
