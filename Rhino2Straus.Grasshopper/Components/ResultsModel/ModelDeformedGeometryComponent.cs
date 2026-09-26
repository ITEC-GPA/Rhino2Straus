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

public sealed class ModelDeformedGeometryComponent : SafeComponent
{
    public ModelDeformedGeometryComponent() : base("Model Deformed Geometry", "Model Deformed Geometry", "Preview of displaced analysis geometry. Scale is visual only; section offsets and thickness are omitted from the deformed centre geometry.", ComponentTopics.Tab, ComponentTopics.ResultsModel) { }
    public override Guid ComponentGuid => new Guid("483b39d4-e09b-550a-9ce7-2007051badc9");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        p.AddIntegerParameter("Case", "Case", "Case", GH_ParamAccess.item, 1);
        p.AddNumberParameter("Scale", "Scale", "Display magnification", GH_ParamAccess.item, 1.0);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddPointParameter("Nodes", "Nodes", "Nodes", GH_ParamAccess.list);
        p.AddCurveParameter("Beams", "Beams", "Beams", GH_ParamAccess.list);
        p.AddMeshParameter("Plates and solids", "Plates and solids", "Plates and solids", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var m=Item<GH_Model>(da,0).Value;var r=m.Results??throw new InvalidOperationException("Model has no results.");int c=Item<int>(da,1);double scale=Item<double>(da,2);if(c<1||!double.IsFinite(scale))throw new ArgumentException("Choose one result case and a finite display scale.");
        var disp=r.Select(m,ResultEntity.Node,ResultQuantity.Displacement,c).ToDictionary(x=>x.EntityId);
        Point3d Position(NodeElementModel n){if(!disp.TryGetValue(n.Id,out var data))throw new InvalidOperationException("Missing displacement for node "+n.Id);var v=data.Values[0];return n.Position+new Vector3d(v[0],v[1],v[2])*scale;}
        da.SetDataList(0,m.NodeElements.Values.Select(Position));var curves=new List<Curve>();
        var beamResults=r.Select(m,ResultEntity.Beam,ResultQuantity.Displacement,c).ToDictionary(x=>x.EntityId);
        foreach(var beam in m.FrameElements.Values){if(beamResults.TryGetValue(beam.Id,out var data)){var delta=beam.NodeEnd.Position-beam.NodeStart.Position;double length=delta.Length;curves.Add(new PolylineCurve(data.Values.Select((v,i)=>beam.NodeStart.Position+delta*(data.Positions[i]/length)+new Vector3d(v[0],v[1],v[2])*scale)));}else curves.Add(new LineCurve(Position(beam.NodeStart),Position(beam.NodeEnd)));}
        var meshes=new List<Mesh>();foreach(var plate in m.AreaElements.Values){var mesh=new Mesh();foreach(var node in plate.NodeList)mesh.Vertices.Add(Position(node));if(plate.NodeList.Count==3)mesh.Faces.AddFace(0,1,2);else mesh.Faces.AddFace(0,1,2,3);mesh.Normals.ComputeNormals();meshes.Add(mesh);}
        foreach(var solid in m.SolidElements.Values){var mesh=solid.ToMesh();for(int i=0;i<solid.Nodes.Count;i++)mesh.Vertices.SetVertex(i,Position(solid.Nodes[i]));mesh.Normals.ComputeNormals();meshes.Add(mesh);}
        da.SetDataList(1,curves);da.SetDataList(2,meshes);
    }
}
