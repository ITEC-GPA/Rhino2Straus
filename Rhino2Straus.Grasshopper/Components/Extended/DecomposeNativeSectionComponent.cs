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

public sealed class DecomposeNativeSectionComponent : SafeComponent
{
    public DecomposeNativeSectionComponent() : base("Decompose Native Beam Section", "Decompose Native Beam Section", "Decompose Native Beam Section", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("a776af15-2871-57ea-8e57-007888c511c1");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Section", "Section", "Section", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddIntegerParameter("Property ID", "Property ID", "Property ID", GH_ParamAccess.item);
        p.AddIntegerParameter("Native property", "Native property", "Native property", GH_ParamAccess.item);
        p.AddNumberParameter("Area", "Area", "Area", GH_ParamAccess.item);
        p.AddNumberParameter("I11", "I11", "I11", GH_ParamAccess.item);
        p.AddNumberParameter("I22", "I22", "I22", GH_ParamAccess.item);
        p.AddNumberParameter("J", "J", "J", GH_ParamAccess.item);
        p.AddNumberParameter("Shear area 1", "Shear area 1", "Shear area 1", GH_ParamAccess.item);
        p.AddNumberParameter("Shear area 2", "Shear area 2", "Shear area 2", GH_ParamAccess.item);
        p.AddNumberParameter("Shear offset 1", "Shear offset 1", "Shear offset 1", GH_ParamAccess.item);
        p.AddNumberParameter("Shear offset 2", "Shear offset 2", "Shear offset 2", GH_ParamAccess.item);
        p.AddPointParameter("Centroid", "Centroid", "Centroid", GH_ParamAccess.item);
        p.AddNumberParameter("Principal angle", "Principal angle", "Radians", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var s=Item<GH_ObjectWrapper>(da,0).Value as NativeBeamSection??throw new ArgumentException("Supply a native section from Model Native Beam Sections.");
        da.SetData(0,s.PropertyId);da.SetData(1,s.NativePropertyNumber);da.SetData(2,s.Area);da.SetData(3,s.I11);da.SetData(4,s.I22);da.SetData(5,s.J);da.SetData(6,s.ShearArea1);da.SetData(7,s.ShearArea2);da.SetData(8,s.ShearOffset1);da.SetData(9,s.ShearOffset2);da.SetData(10,new Point3d(s.CentroidX,s.CentroidY,0));da.SetData(11,s.PrincipalAngle);
    }
}
