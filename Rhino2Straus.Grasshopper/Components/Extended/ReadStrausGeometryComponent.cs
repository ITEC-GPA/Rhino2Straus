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

public sealed class ReadStrausGeometryComponent : SafeComponent
{
    public ReadStrausGeometryComponent() : base("Read Straus Geometry", "Read Straus Geometry", "Reads geometry in the file length units. Element numbers are list index + 1; IDs are separate labels. Does not import loads or properties into a new FEM model.", "Rhino2Straus", "10-Import") { }
    public override Guid ComponentGuid => new Guid("7cfff3d7-4b55-51be-83a2-0edce3eb3ad0");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Model path", "Model path", "Model path", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddPointParameter("Nodes", "Nodes", "Nodes", GH_ParamAccess.list);
        p.AddIntegerParameter("Node numbers", "Node numbers", "Node numbers", GH_ParamAccess.list);
        p.AddIntegerParameter("Node IDs", "Node IDs", "Node IDs", GH_ParamAccess.list);
        p.AddLineParameter("Beams", "Beams", "Beams", GH_ParamAccess.list);
        p.AddIntegerParameter("Beam IDs", "Beam IDs", "Beam IDs", GH_ParamAccess.list);
        p.AddMeshParameter("Plates", "Plates", "Plates", GH_ParamAccess.list);
        p.AddIntegerParameter("Plate IDs", "Plate IDs", "Plate IDs", GH_ParamAccess.list);
        p.AddIntegerParameter("Units", "Units", "Straus7 length, force, stress, mass, temperature and energy codes", GH_ParamAccess.list);
        p.AddTextParameter("Load cases", "Load cases", "Load cases", GH_ParamAccess.list);
        p.AddMeshParameter("Solids","Solids","Linear solid skins",GH_ParamAccess.list);p.AddIntegerParameter("Solid IDs","Solid IDs","Solid labels",GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var g=StrausFileService.ReadGeometry(Item<string>(da,0)); var points=g.Nodes.ToDictionary(n=>n.Number,n=>n.Position);
        da.SetDataList(0,g.Nodes.Select(n=>n.Position)); da.SetDataList(1,g.Nodes.Select(n=>n.Number)); da.SetDataList(2,g.Nodes.Select(n=>n.Id));
        da.SetDataList(3,g.Beams.Select(b=>new Line(points[b.Nodes[0]],points[b.Nodes[1]]))); da.SetDataList(4,g.Beams.Select(b=>b.Id));
        var meshes=new List<Mesh>(); foreach(var a in g.Plates) { if(a.Nodes.Length!=3 && a.Nodes.Length!=4) throw new NotSupportedException("Geometry preview supports Tri3/Quad4 plates only.");
            var m=new Mesh(); foreach(int n in a.Nodes)m.Vertices.Add(points[n]); if(a.Nodes.Length==3)m.Faces.AddFace(0,1,2);else m.Faces.AddFace(0,1,2,3); m.Normals.ComputeNormals(); meshes.Add(m); }
        da.SetDataList(5,meshes); da.SetDataList(6,g.Plates.Select(p=>p.Id)); da.SetDataList(7,g.Units); da.SetDataList(8,g.LoadCases);
        da.SetDataList(9,g.Solids.Select(s=>new SolidElementModel {Nodes=s.Nodes.Select(n=>new NodeElementModel(points[n])).ToList()}.ToMesh()));da.SetDataList(10,g.Solids.Select(s=>s.Id));
        Message=$"{g.Nodes.Length} N / {g.Beams.Length} B / {g.Plates.Length} P";
    }
}
