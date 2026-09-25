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



public sealed class GlobalPressureComponent : SafeComponent

{

    public GlobalPressureComponent() : base("Plate Global Pressure", "Plate Global Pressure", "Plate Global Pressure", "Rhino2Straus", "06-Loads") { }

    public override Guid ComponentGuid => new Guid("a94608dd-cbf9-5588-a5dd-e09c157e9bbe");

    protected override void RegisterInputParams(GH_InputParamManager p)

    {

        p.AddGenericParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);

        p.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);

        p.AddVectorParameter("Pressure", "Pressure", "Global XYZ pressure, force/length²", GH_ParamAccess.item);

        p.AddBooleanParameter("Top face", "Top face", "Top face", GH_ParamAccess.item, true);

        p.AddBooleanParameter("Projected", "Projected", "Projected", GH_ParamAccess.item, false);

    }

    protected override void RegisterOutputParams(GH_OutputParamManager p)

    {

        p.AddGenericParameter("Plate", "Plate", "Plate", GH_ParamAccess.item);

    }

    protected override void Solve(IGH_DataAccess da)

    {



        var a=new AreaElementModel(Item<GH_AreaElement>(da,0).Value); var v=Item<Vector3d>(da,2);

        if (!v.IsValid) throw new ArgumentException("Invalid pressure vector.");

        a.AreaLoadList.Add(new AreaGlobalPressureStraus(Item<GH_LoadCase>(da,1).Value,Item<bool>(da,3)?AreaGlobalPressureStraus.LoadFace.Top:AreaGlobalPressureStraus.LoadFace.Bottom,CoordinateSystemModel.Global,v.X,v.Y,v.Z,Item<bool>(da,4)));

        da.SetData(0,new GH_AreaElement(a));

    }

}
