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

public sealed class NodeSpringComponent : SafeComponent
{
    public NodeSpringComponent() : base("Node Springs", "Node Springs", "Node Springs", ComponentTopics.Tab, ComponentTopics.ConstraintsLinks) { }
    public override Guid ComponentGuid => new Guid("55f8ebd6-db14-53d5-ada3-45d2fe35e99e");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
        p.AddVectorParameter("Translation", "Translation", "XYZ stiffnesses in force/length", GH_ParamAccess.item, Vector3d.Zero);
        p.AddVectorParameter("Rotation", "Rotation", "XYZ rotational stiffnesses in moment/radian", GH_ParamAccess.item, Vector3d.Zero);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var n=new NodeElementModel(Item<GH_NodeElement>(da,0).Value); var t=Item<Vector3d>(da,1); var r=Item<Vector3d>(da,2);
        if (!t.IsValid || !r.IsValid || new[]{t.X,t.Y,t.Z,r.X,r.Y,r.Z}.Any(x=>x<0)) throw new ArgumentException("Values must be finite and nonnegative.");
        n.TranslationSpring=t; n.RotationSpring=r; da.SetData(0,new GH_NodeElement(n));
    }
}
