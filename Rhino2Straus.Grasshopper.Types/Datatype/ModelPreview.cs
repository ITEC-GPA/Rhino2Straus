using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rhino.Display;
using Rhino.Geometry;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Models;
using Rhino2Straus.Core.Settings;
namespace Rhino2Straus.Grasshopper.Datatype;

public sealed class ModelPreview
{
    public List<Point3d> Nodes { get; } = new();
    public List<Line> Lines { get; } = new();
    public List<Brep> Beams { get; } = new();
    public List<Mesh> Plates { get; } = new();
    public List<Mesh> Solids { get; } = new();
    public ModelAnnotations Annotations { get; }
    public PreviewSettings Settings { get; }
    public BoundingBox Bounds { get; private set; }=BoundingBox.Empty;
    private readonly List<(PreviewMark Mark,Curve[] Curves,Line? Arrow,Point3d Label,int Row)> symbols=new();
    public ModelPreview(ModelModel model,double axisScale=1):this(model,new PreviewSettings{SymbolScale=axisScale}) { }
    public ModelPreview(ModelModel model,PreviewSettings settings,IEnumerable<string> cases=null)
    {
        Settings=settings;settings.Validate();if(model==null)return;
        Annotations=new ModelAnnotations(model,settings,cases);if(!settings.Enabled)return;
        if(settings.Shows(PreviewLayer.Nodes))Nodes.AddRange(model.NodeElements.Values.Select(n=>n.Position));
        if(settings.Shows(PreviewLayer.Beams))foreach(var beam in model.FrameElements.Values)
        {
            Lines.Add(new Line(beam.NodeStart.Position,beam.NodeEnd.Position));
            if(settings.Shows(PreviewLayer.Sections))Beams.AddRange(PreviewGeometry.Beam(beam));
        }
        if(settings.Shows(PreviewLayer.Sections)&&settings.Shows(PreviewLayer.Beams))
        {
            int missing=model.FrameElements.Values.Count(b=>b.FrameSection?.Breps==null||b.FrameSection.Breps.Count==0);
            if(missing>0)Annotations.Warnings.Add($"{missing} beam sections have no profile; only their analysis lines are visible. Supply a planar profile for generic sections.");
        }
        if(settings.Shows(PreviewLayer.Plates))foreach(var plate in model.AreaElements.Values)Plates.Add(PreviewGeometry.Plate(plate,settings.Shows(PreviewLayer.Sections)));
        if(settings.Shows(PreviewLayer.Solids))Solids.AddRange(model.SolidElements.Values.Select(s=>s.ToMesh()));
        if(settings.Shows(PreviewLayer.Links))Lines.AddRange(model.LinkElements.Values.Select(l=>new Line(l.NodeStart.Position,l.NodeEnd.Position)));
        var box=BoundingBox.Empty;
        foreach(var n in Nodes)box.Union(n);foreach(var l in Lines)box.Union(l.BoundingBox);
        foreach(var b in Beams)box.Union(b.GetBoundingBox(true));foreach(var mesh in Plates.Concat(Solids))box.Union(mesh.GetBoundingBox(true));
        var labelRows=new Dictionary<(string,Point3d),int>();
        foreach(var mark in Annotations.Marks)
        {
            var glyph=BuildSymbol(mark,settings);var key=(mark.Owner,mark.Point);
            labelRows.TryGetValue(key,out int row);labelRows[key]=row+1;
            var label=glyph.Label;
            symbols.Add((mark,glyph.Curves,glyph.Arrow,label,row));box.Union(mark.Point);box.Union(label);
            foreach(var c in glyph.Curves)box.Union(c.GetBoundingBox(true));if(glyph.Arrow.HasValue)box.Union(glyph.Arrow.Value.BoundingBox);
        }
        Bounds=box;
    }
    public static (Curve[] Curves,Line? Arrow,Point3d Label) BuildSymbol(PreviewMark mark,PreviewSettings settings)
    {
        var curves=new List<Curve>();var p=mark.Point;var direction=mark.Vector;double size=settings.SymbolScale;
        if(!direction.Unitize())direction=Vector3d.ZAxis;
        var reference=Math.Abs(direction*Vector3d.ZAxis)<.9?Vector3d.ZAxis:Vector3d.XAxis;
        var side=Vector3d.CrossProduct(direction,reference);side.Unitize();var second=Vector3d.CrossProduct(direction,side);
        double magnitude=Math.Min(size*6,Math.Max(size*.15,mark.Vector.Length*settings.LoadScale));
        if(mark.Symbol==PreviewSymbol.Axis)return(Array.Empty<Curve>(),new Line(p,p+direction*size),p+direction*size);
        if(mark.Symbol==PreviewSymbol.Arrow)return(Array.Empty<Curve>(),new Line(p-direction*magnitude,p),p-direction*magnitude);
        if(mark.Symbol==PreviewSymbol.Offset)return(Array.Empty<Curve>(),new Line(p,p+mark.Vector),p+mark.Vector*.5);
        if(mark.Symbol==PreviewSymbol.Moment)
        {
            double radius=size*.3;var center=p-direction*size*.25;
            var points=Enumerable.Range(0,25).Select(i=>{double angle=i*Math.PI*1.6/24;return center+(side*Math.Cos(angle)+second*Math.Sin(angle))*radius;}).ToArray();
            curves.Add(new PolylineCurve(points));return(curves.ToArray(),new Line(points[^2],points[^1]),center+side*radius);
        }
        if(mark.Symbol==PreviewSymbol.Support)
        {
            var end=p-direction*size*.3;curves.Add(new PolylineCurve(new[]{p,end+side*size*.13,end-side*size*.13,p}));
            if(mark.Text.Contains(" R"))curves.Add(new Circle(new Plane(end,direction),size*.08).ToNurbsCurve());
        }
        else if(mark.Symbol==PreviewSymbol.Release)curves.Add(new Circle(new Plane(p,direction),size*.12).ToNurbsCurve());
        else if(mark.Symbol==PreviewSymbol.Spring)
        {
            var points=new List<Point3d>{p};for(int i=1;i<=8;i++)points.Add(p-direction*(i*size*.05)+side*(i%2==0?-.05:.05)*size);points.Add(p-direction*size*.45);curves.Add(new PolylineCurve(points));
        }
        else if(mark.Symbol==PreviewSymbol.Mass)
        {
            curves.Add(new Circle(new Plane(p+direction*size*.16,direction),size*.1).ToNurbsCurve());curves.Add(new LineCurve(p,p+direction*size*.16));
        }
        else if(mark.Symbol==PreviewSymbol.Text&&mark.Layer==PreviewLayer.Loads)
            curves.Add(new PolylineCurve(new[]{p+side*size*.1,p+second*size*.1,p-side*size*.1,p-second*size*.1,p+side*size*.1}));
        return(curves.ToArray(),null,p+side*size*.18);
    }
    public void DrawWires(DisplayPipeline display,Color color)
    {
        if(!Settings.Enabled)return;
        foreach(var n in Nodes)display.DrawPoint(n,color);foreach(var l in Lines)display.DrawLine(l,color);
        foreach(var b in Beams)display.DrawBrepWires(b,color);foreach(var p in Plates.Concat(Solids))display.DrawMeshWires(p,color);
        foreach(var symbol in symbols)
        {
            var mark=symbol.Mark;foreach(var curve in symbol.Curves)display.DrawCurve(curve,mark.Color,2);
            if(symbol.Arrow is {} arrow)display.DrawArrow(arrow,mark.Color);
            if(mark.Layer is PreviewLayer.Labels or PreviewLayer.Properties || Settings.Shows(PreviewLayer.Values))
            {
                var point=display.Viewport.WorldToClient(symbol.Label);
                display.Draw2dText(mark.Text,mark.Color,new Point2d(point.X+4,point.Y+symbol.Row*14),false,11);
            }
        }
    }
    public void DrawMeshes(DisplayPipeline display,DisplayMaterial material)
    {
        if(!Settings.Enabled||!Settings.Shaded)return;
        foreach(var b in Beams)display.DrawBrepShaded(b,material);foreach(var p in Plates.Concat(Solids))display.DrawMeshShaded(p,material);
    }
}
