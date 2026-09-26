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



public sealed class MaterialInfoComponent : SafeComponent

{

    public MaterialInfoComponent() : base("Deconstruct Material", "Deconstruct Material", "Deconstruct Material", ComponentTopics.Tab, ComponentTopics.Materials) { }

    public override Guid ComponentGuid => new Guid("e1fef21a-c3c1-5759-a878-b4193a285068");

    protected override void RegisterInputParams(GH_InputParamManager p)

    {

        p.AddGenericParameter("Material", "Material", "Material", GH_ParamAccess.item);

    }

    protected override void RegisterOutputParams(GH_OutputParamManager p)

    {

        p.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);

        p.AddNumberParameter("E", "E", "kPa", GH_ParamAccess.item);

        p.AddNumberParameter("G", "G", "kPa", GH_ParamAccess.item);

        p.AddNumberParameter("Poisson", "Poisson", "Poisson", GH_ParamAccess.item);

        p.AddNumberParameter("Density", "Density", "tonne/m³", GH_ParamAccess.item);

        p.AddNumberParameter("Expansion", "Expansion", "Expansion", GH_ParamAccess.item);

        p.AddNumberParameter("fy","fy","Nominal yield strength in MPa; 0 means not defined",GH_ParamAccess.item);

        p.AddNumberParameter("fu","fu","Nominal tensile strength in MPa; 0 means not defined",GH_ParamAccess.item);

        p.AddNumberParameter("fck","fck","Nominal compressive strength in MPa; 0 means not defined",GH_ParamAccess.item);



    }

    protected override void Solve(IGH_DataAccess da)

    {



        var m=Item<Rhino2Straus.Grasshopper.Datatype.GH_Material>(da,0).Value; da.SetData(0,m.Name); da.SetData(1,m.ModulusElasticity); da.SetData(2,m.ShearModulus); da.SetData(3,m.PoissonRatio); da.SetData(4,m.Density); da.SetData(5,m.ThermalCoefficient);da.SetData(6,m.TensileYeld);da.SetData(7,m.TensileStrength);da.SetData(8,m.CompressiveStrength);

    }

}
