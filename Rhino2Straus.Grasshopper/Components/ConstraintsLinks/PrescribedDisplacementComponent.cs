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

public sealed class PrescribedDisplacementComponent : SafeComponent
{
    public PrescribedDisplacementComponent() : base("Node Prescribed Displacement", "Node Prescribed Displacement", "Node Prescribed Displacement", ComponentTopics.Tab, ComponentTopics.ConstraintsLinks) { }
    public override Guid ComponentGuid => new Guid("c1f82608-ecdc-5f6d-84b8-b840b880bff2");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
        p.AddBooleanParameter("Restrained", "Restrained", "Six flags: UX UY UZ RX RY RZ", GH_ParamAccess.list);
        p.AddNumberParameter("Values", "Values", "Six values: model length, then radians; freedom case 1", GH_ParamAccess.list);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var flags=new List<bool>();var values=new List<double>();da.GetDataList(1,flags);da.GetDataList(2,values);
        if(flags.Count!=6||values.Count!=6||values.Any(x=>!double.IsFinite(x)))throw new ArgumentException("Supply six restraint flags and six finite displacement/rotation values.");
        var n=new NodeElementModel(Item<GH_NodeElement>(da,0).Value);
        n.Support=new NodeSupportModel {Dx=flags[0],Dy=flags[1],Dz=flags[2],Mx=flags[3],My=flags[4],Mz=flags[5]};
        n.PrescribedDisplacement=values.Select((v,i)=>flags[i]?v:0).ToArray();da.SetData(0,new GH_NodeElement(n));
    }
}
