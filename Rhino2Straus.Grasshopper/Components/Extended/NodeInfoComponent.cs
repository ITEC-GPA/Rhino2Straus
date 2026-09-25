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

public sealed class NodeInfoComponent : SafeComponent
{
    public NodeInfoComponent() : base("Deconstruct Node", "Deconstruct Node", "Deconstruct Node", "Rhino2Straus", "09-Tools") { }
    public override Guid ComponentGuid => new Guid("d48c8008-78bb-5413-8fcc-d7dcf512d44d");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddIntegerParameter("ID", "ID", "ID", GH_ParamAccess.item);
        p.AddPointParameter("Point", "Point", "Point", GH_ParamAccess.item);
        p.AddBooleanParameter("Restraints", "Restraints", "DX,DY,DZ,RX,RY,RZ", GH_ParamAccess.list);
        p.AddIntegerParameter("Loads", "Loads", "Loads", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var n=Item<GH_NodeElement>(da,0).Value; var s=n.Support; da.SetData(0,n.Id); da.SetData(1,n.Position); da.SetDataList(2,new[]{s.Dx,s.Dy,s.Dz,s.Mx,s.My,s.Mz}); da.SetData(3,n.NodalLoadList.Count+n.Temperatures.Count);
    }
}
