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

public sealed class BakeStrausGeometryComponent : SafeComponent
{
    private bool wasRun; private List<string> ids=new();
    public BakeStrausGeometryComponent() : base("Bake Straus Geometry", "Bake Straus Geometry", "Bake Straus Geometry", "Rhino2Straus", "10-Import") { }
    public override Guid ComponentGuid => new Guid("1fa3c129-3308-46c8-a529-67d87502956c");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Model path", "Model path", "Model path", GH_ParamAccess.item);
        p.AddBooleanParameter("Bake", "Bake", "Use a button or a false-to-true transition", GH_ParamAccess.item, false);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddTextParameter("Object IDs", "Object IDs", "Object IDs", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        bool run=Item<bool>(da,1); bool fire=run&&!wasRun;wasRun=run;
        if(fire){var doc=RhinoDoc.ActiveDoc??throw new InvalidOperationException("No active Rhino document.");
            var g=StrausFileService.ReadGeometry(Item<string>(da,0));
            var fileUnit=g.Units[St7.ipLENGTHU] switch {St7.luMETRE=>UnitSystem.Meters,St7.luCENTIMETRE=>UnitSystem.Centimeters,St7.luMILLIMETRE=>UnitSystem.Millimeters,St7.luFOOT=>UnitSystem.Feet,St7.luINCH=>UnitSystem.Inches,_=>throw new NotSupportedException("Unknown model length unit.")};
            double scale=RhinoMath.UnitScale(fileUnit,doc.ModelUnitSystem);var points=g.Nodes.ToDictionary(n=>n.Number,n=>new Point3d(n.Position.X*scale,n.Position.Y*scale,n.Position.Z*scale));
            if(g.Plates.Any(a=>a.Nodes.Length!=3 && a.Nodes.Length!=4))throw new NotSupportedException("Bake supports Tri3/Quad4 plates only.");
            var created=new List<Guid>();uint undo=doc.BeginUndoRecord("Bake Straus7 geometry");
            try { foreach(var n in g.Nodes)created.Add(doc.Objects.AddPoint(points[n.Number]));
                foreach(var b in g.Beams)created.Add(doc.Objects.AddLine(points[b.Nodes[0]],points[b.Nodes[1]]));
                foreach(var a in g.Plates){var m=new Mesh();foreach(int n in a.Nodes)m.Vertices.Add(points[n]);if(a.Nodes.Length==3)m.Faces.AddFace(0,1,2);else m.Faces.AddFace(0,1,2,3);m.Normals.ComputeNormals();created.Add(doc.Objects.AddMesh(m));}
                foreach(var s in g.Solids)created.Add(doc.Objects.AddMesh(new SolidElementModel{Nodes=s.Nodes.Select(n=>new NodeElementModel(points[n])).ToList()}.ToMesh()));
                if(created.Any(id=>id==Guid.Empty))throw new InvalidOperationException("Rhino rejected a geometry object.");ids=created.Select(x=>x.ToString()).ToList();}
            catch {foreach(var id in created.Where(x=>x!=Guid.Empty))doc.Objects.Delete(id,true);throw;}
            finally{doc.EndUndoRecord(undo);doc.Views.Redraw();}}
        da.SetDataList(0,ids);
    }
}
