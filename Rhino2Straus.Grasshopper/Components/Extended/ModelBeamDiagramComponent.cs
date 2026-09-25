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

public sealed class ModelBeamDiagramComponent : SafeComponent
{
    public ModelBeamDiagramComponent() : base("Model Beam Force Diagram", "Model Beam Force Diagram", "Model Beam Force Diagram", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("98ea7e29-bea7-5aa2-8dec-f05e46c51cd8");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        p.AddIntegerParameter("Case", "Case", "Case", GH_ParamAccess.item, 1);
        p.AddTextParameter("Action", "Action", "N, V1, V2, T, M1, M2", GH_ParamAccess.item, "M1");
        p.AddNumberParameter("Scale", "Scale", "Model length per force/moment unit", GH_ParamAccess.item, 0.01);
        p.AddIntegerParameter("Display axis", "Display axis", "Principal axis 1 or 2", GH_ParamAccess.item, 2);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddCurveParameter("Diagrams", "Diagrams", "Diagrams", GH_ParamAccess.tree);
        p.AddNumberParameter("Values", "Values", "Values", GH_ParamAccess.tree);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var m=Item<GH_Model>(da,0).Value;var r=m.Results??throw new InvalidOperationException("Model has no results.");int c=Item<int>(da,1),axis=Item<int>(da,4);string action=Item<string>(da,2);double scale=Item<double>(da,3);
        if(c<1||(axis!=1&&axis!=2)||!double.IsFinite(scale))throw new ArgumentException("Choose one case, display axis 1/2 and a finite scale.");
        var curves=new GH_Structure<GH_Curve>();var numbers=new GH_Structure<GH_Number>();
        foreach(var data in r.Select(m,ResultEntity.Beam,ResultQuantity.Force,c)){int col=data.Columns.IndexOf(action);if(col<0)throw new ArgumentException("Unknown action "+action);var beam=m.FrameElements[data.EntityId];beam.GetLocalAxes(true,out var z,out var y,out var x,out _);var direction=axis==1?x:y;
        var points=new List<Point3d>{beam.NodeStart.Position};points.AddRange(data.Values.Select((v,i)=>beam.NodeStart.Position+z*data.Positions[i]+direction*v[col]*scale));points.Add(beam.NodeEnd.Position);var path=new GH_Path(c,beam.Id);curves.Append(new GH_Curve(new PolylineCurve(points)),path);numbers.AppendRange(data.Values.Select(v=>new GH_Number(v[col])),path);}
        da.SetDataTree(0,curves);da.SetDataTree(1,numbers);
    }
}
