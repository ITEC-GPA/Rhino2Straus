using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Rhino2Straus.Grasshopper.Datatype;

internal static class BrepCast
{
    // A Grasshopper cast must return one object. Never silently drop other section parts.
    internal static bool Try<Q>(Func<IReadOnlyList<Brep>> create,out Q target)
    {
        target=default;
        if(typeof(Q)!=typeof(GH_Brep)&&typeof(Q)!=typeof(Brep))return false;
        try
        {
            var breps=create();if(breps.Count!=1)return false;
            target=typeof(Q)==typeof(GH_Brep)?(Q)(object)new GH_Brep(breps[0]):(Q)(object)breps[0];
            return true;
        }
        catch(Exception error) when(error is ArgumentException or InvalidOperationException){return false;}
    }
}
