using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Loads;
using Rhino2Straus.Core.Models;
using Rhino2Straus.Core.Settings;
using Rhino2Straus.Grasshopper.Datatype;
using Rhino2Straus.Import;
using St7API;

namespace Rhino2Straus.Grasshopper.Components.Extended;

public sealed class LoadAreaUniformComponent : SafeComponent
{
    public LoadAreaUniformComponent() : base("Area Load Uniform", "Area Load Uniform", "Area Load Uniform", "Rhino2Straus", "06-Loads") { }
    public override Guid ComponentGuid => new Guid("5ce9edca-2f7d-4b4d-902a-d31986b8a6c7");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Area", "Area", "Area", GH_ParamAccess.item);
        p.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
        p.AddIntegerParameter("Direction", "Direction", "0=L1, 1=L2, 2=normal +Z, 3=GX, 4=GY, 5=GZ", GH_ParamAccess.item, 5);
        p.AddBooleanParameter("Projected", "Projected", "Projected", GH_ParamAccess.item, false);
        p.AddNumberParameter("Load", "Load", "Force per model length squared", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Area", "Area", "Area", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var a = new AreaElementModel(Item<GH_AreaElement>(da,0).Value); var c = Item<GH_LoadCase>(da,1).Value;
        int direction=Item<int>(da,2); bool projected=Item<bool>(da,3); double value=Item<double>(da,4);
        if (direction < 0 || direction > 5 || !double.IsFinite(value)) throw new ArgumentException("Invalid pressure.");
        if (direction < 3 && projected) throw new ArgumentException("Projection requires a global direction.");
        if (direction == 2) a.AreaLoadList.Add(new AreaPressureLoadModel(c,value));
        else { a.GetLocalAxes(out var x, out var y, out var z);
            var axis = direction switch { 0 => x, 1 => y, 3 => Vector3d.XAxis, 4 => Vector3d.YAxis, _ => Vector3d.ZAxis };
            var force=axis*value; a.AreaLoadList.Add(new AreaGlobalPressureStraus(c,AreaGlobalPressureStraus.LoadFace.Bottom,CoordinateSystemModel.Global,force.X,force.Y,force.Z,projected)); }
        da.SetData(0,new GH_AreaElement(a));
    }
}
