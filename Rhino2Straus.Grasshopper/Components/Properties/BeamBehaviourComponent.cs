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

public sealed class BeamBehaviourComponent : SafeComponent
{
    public BeamBehaviourComponent() : base("Beam Behaviour", "Beam Behaviour", "Beam Behaviour", ComponentTopics.Tab, ComponentTopics.Properties) { }
    public override Guid ComponentGuid => new Guid("a4e2d42d-21ca-596e-a98d-47396bfbae2a");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Property", "Property", "Property", GH_ParamAccess.item);
        p.AddIntegerParameter("Behaviour", "Behaviour", "0=Beam, 1=Truss, 2=Cable; cable requires nonlinear analysis", GH_ParamAccess.item, 0);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Property", "Property", "Property", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        int b=Item<int>(da,1);if(b<0||b>2)throw new ArgumentException("Behaviour must be 0, 1 or 2.");
        var p=new FramePropertyModel(Item<GH_FrameProperty>(da,0).Value) {Behaviour=(FramePropertyModel.Behaviours)b};
        p.Name += " " + p.Behaviour; p.Id=-1; da.SetData(0,new GH_FrameProperty(p));
    }
}
