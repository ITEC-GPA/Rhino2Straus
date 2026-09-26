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

public sealed class SolidElementComponent : SafeComponent
{
    public SolidElementComponent() : base("Solid Element", "Solid Element", "Solid Element", ComponentTopics.Tab, ComponentTopics.Elements) { }
    public override Guid ComponentGuid => new Guid("f7fcbe06-f2a8-549b-9872-e14da102ec5b");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Nodes", "Nodes", "4, 6 or 8 nodes in Straus7 local order", GH_ParamAccess.list);
        p.AddGenericParameter("Material", "Material", "Material", GH_ParamAccess.item);
        p.AddIntegerParameter("ID", "ID", "ID", GH_ParamAccess.item, -1);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Solid", "Solid", "Solid", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var nodes=new List<GH_NodeElement>();da.GetDataList(0,nodes);if(nodes.Count is not (4 or 6 or 8))throw new ArgumentException("Use Tet4, Wedge6 or Hexa8 connectivity.");
        var solid=new SolidElementModel{Nodes=nodes.Select(x=>x.Value).ToList(),Material=Item<Rhino2Straus.Grasshopper.Datatype.GH_Material>(da,1).Value,Id=Item<int>(da,2)};
        if(solid.SignedVolume()<=0)throw new ArgumentException("Solid connectivity is inverted or degenerate.");da.SetData(0,new GH_SolidElement(solid));
    }
}
