using System.Collections.Generic;

using System.Drawing;

using System.Linq;

using Grasshopper.Kernel;

using Rhino.Display;

using Rhino.Geometry;

using Rhino2Straus.Core.Helper;

using Rhino2Straus.Core.Models;



namespace Rhino2Straus.Grasshopper.Datatype;



public sealed class ModelPreview

{

    public List<Point3d> Nodes { get; } = new();

    public List<Line> Lines { get; } = new();

    public List<Brep> Beams { get; } = new();

    public List<Mesh> Plates { get; } = new();

    public List<Mesh> Solids { get; } = new();

    public List<(Point3d Point, string Text)> Labels { get; } = new();

    public List<Line> Axis1 { get; } = new();

    public List<Line> Axis2 { get; } = new();

    public List<Line> Axis3 { get; } = new();

    public List<Point3d> Supports { get; } = new();

    public BoundingBox Bounds { get; private set; } = BoundingBox.Empty;



    public ModelPreview(ModelModel model, double axisScale = 1)

    {

        if (model == null) return;

        foreach (var n in model.NodeElements.Values) { Nodes.Add(n.Position); Labels.Add((n.Position,"N"+n.Id)); if(n.Support.IsActive)Supports.Add(n.Position); }

        foreach (var b in model.FrameElements.Values)

        {

            Lines.Add(new Line(b.NodeStart.Position,b.NodeEnd.Position)); Beams.AddRange(PreviewGeometry.Beam(b));

            var center=(b.NodeStart.Position+b.NodeEnd.Position)/2;

            b.GetLocalAxes(true,out var z,out var y,out var x,out _);

            Axis1.Add(new Line(center,center+x*axisScale));Axis2.Add(new Line(center,center+y*axisScale));Axis3.Add(new Line(center,center+z*axisScale));

            Labels.Add((center,$"B{b.Id} Â· {b.FrameProperty.Name}"));

        }

        foreach (var p in model.AreaElements.Values)

        {

            Plates.Add(PreviewGeometry.Plate(p));

            var center=new Point3d(p.NodeList.Average(n=>n.X),p.NodeList.Average(n=>n.Y),p.NodeList.Average(n=>n.Z));

            p.GetLocalAxes(out var x,out var y,out var z);Axis1.Add(new Line(center,center+x*axisScale));Axis2.Add(new Line(center,center+y*axisScale));Axis3.Add(new Line(center,center+z*axisScale));

            Labels.Add((center,$"P{p.Id} Â· {p.AreaProperty.Name}"));

        }

        foreach(var s in model.SolidElements.Values){Solids.Add(s.ToMesh());Labels.Add((s.Nodes[0].Position,$"S{s.Id} · {s.Material.Name}"));}

        foreach(var l in model.LinkElements.Values)Lines.Add(new Line(l.NodeStart.Position,l.NodeEnd.Position));

        var box=new BoundingBox(Nodes);foreach(var b in Beams)box.Union(b.GetBoundingBox(true));foreach(var p in Plates.Concat(Solids))box.Union(p.GetBoundingBox(true));

        foreach(var l in Axis1.Concat(Axis2).Concat(Axis3))box.Union(l.BoundingBox);Bounds=box;

    }



    public void DrawWires(DisplayPipeline display, Color color, bool labels = false, bool axes = false, bool supports = true, bool physical = true)

    {

        foreach(var n in Nodes)display.DrawPoint(n,color);

        foreach(var l in Lines)display.DrawLine(l,color);

        if(physical) {foreach(var b in Beams)display.DrawBrepWires(b,color);foreach(var p in Plates.Concat(Solids))display.DrawMeshWires(p,color);}

        if(labels)foreach(var label in Labels)display.Draw2dText(label.Text,color,label.Point,false,12);

        if(axes){foreach(var l in Axis1)display.DrawArrow(l,Color.Red);foreach(var l in Axis2)display.DrawArrow(l,Color.Green);foreach(var l in Axis3)display.DrawArrow(l,Color.Blue);}

        if(supports)foreach(var p in Supports)display.DrawPoint(p,PointStyle.Square,5,Color.OrangeRed);

    }

    public void DrawMeshes(DisplayPipeline display, DisplayMaterial material)

    { foreach(var b in Beams)display.DrawBrepShaded(b,material);foreach(var p in Plates.Concat(Solids))display.DrawMeshShaded(p,material); }

}
