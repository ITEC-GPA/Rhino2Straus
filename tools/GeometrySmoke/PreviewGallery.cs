using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Rhino.Geometry;
using Rhino2Straus.Core.Models;
using Rhino2Straus.Core.Settings;
using Rhino2Straus.Grasshopper.Datatype;

/// <summary>Deterministic orthographic projection of the actual preview primitives; no viewport/GPU capture.</summary>
internal static class PreviewGallery
{
    public static void Save(ModelModel model,string path)
    {
        using var bitmap=new Bitmap(1440,1060);using var g=Graphics.FromImage(bitmap);g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(Color.FromArgb(240,245,248));
        using var title=new Font("Segoe UI",32,FontStyle.Bold,GraphicsUnit.Pixel);using var heading=new Font("Segoe UI",24,FontStyle.Bold,GraphicsUnit.Pixel);using var text=new Font("Segoe UI",16,FontStyle.Regular,GraphicsUnit.Pixel);
        g.DrawString("Rhino2Straus | Preview layers",title,Brushes.DarkSlateGray,28,18);
        g.DrawString("Projection of the same geometry and symbols used by the viewport. Illustrative model; values hidden for clarity.",text,Brushes.DimGray,30,62);
        var geometry=PreviewLayer.Nodes|PreviewLayer.Beams|PreviewLayer.Plates|PreviewLayer.Solids|PreviewLayer.Links;
        var panels=new[]{("Sections and geometry",new PreviewSettings{Layers=geometry|PreviewLayer.Sections,SymbolScale=.4,LoadScale=.15}),
            ("Loads and load directions",new PreviewSettings{Layers=geometry|PreviewLayer.Loads,SymbolScale=.4,LoadScale=.15}),
            ("Attributes and local axes",new PreviewSettings{Layers=geometry|PreviewLayer.Supports|PreviewLayer.Releases|PreviewLayer.Springs|PreviewLayer.Masses|PreviewLayer.Prescribed|PreviewLayer.Offsets|PreviewLayer.Axes,SymbolScale=.4,LoadScale=.15}),
            ("Master visibility OFF",new PreviewSettings{Enabled=false})};
        for(int panel=0;panel<panels.Length;panel++)
        {
            int left=24+(panel%2)*708,top=102+(panel/2)*460;g.FillRectangle(Brushes.White,left,top,684,438);g.DrawString(panels[panel].Item1,heading,Brushes.DarkSlateGray,new RectangleF(left+18,top+14,650,52));
            var scene=new ModelPreview(model,panels[panel].Item2);var segments=new List<(Point3d A,Point3d B,Color Color,bool Arrow)>();
            void CurveLines(Curve curve,Color color){var t=curve.DivideByCount(40,true);if(t==null)return;var points=t.Select(curve.PointAt).ToArray();for(int i=1;i<points.Length;i++)segments.Add((points[i-1],points[i],color,false));}
            void Edge(Line line,Color color,bool arrow=false)=>segments.Add((line.From,line.To,color,arrow));
            foreach(var line in scene.Lines)Edge(line,Color.SteelBlue);
            foreach(var brep in scene.Beams)foreach(var edge in brep.Edges)CurveLines(edge,Color.SteelBlue);
            foreach(var mesh in scene.Plates.Concat(scene.Solids))for(int i=0;i<mesh.TopologyEdges.Count;i++)Edge(mesh.TopologyEdges.EdgeLine(i),Color.SteelBlue);
            foreach(var mark in scene.Annotations.Marks){var symbol=ModelPreview.BuildSymbol(mark,scene.Settings);foreach(var curve in symbol.Curves)CurveLines(curve,mark.Color);if(symbol.Arrow is {} line)Edge(line,mark.Color,true);}
            PointF Project(Point3d p)=>new((float)(p.X+.55*p.Y),(float)(.25*p.X-.45*p.Y-1.1*p.Z));
            var projected=segments.SelectMany(l=>new[]{Project(l.A),Project(l.B)}).ToArray();
            if(projected.Length==0){g.DrawString("Geometry, loads, attributes and managed previews hidden",text,Brushes.DimGray,left+46,top+215);continue;}
            float minX=projected.Min(p=>p.X),maxX=projected.Max(p=>p.X),minY=projected.Min(p=>p.Y),maxY=projected.Max(p=>p.Y);
            float scale=Math.Min(610/Math.Max(maxX-minX,.01f),320/Math.Max(maxY-minY,.01f));
            PointF Screen(Point3d p){var q=Project(p);return new(left+342+(q.X-(minX+maxX)/2)*scale,top+244+(q.Y-(minY+maxY)/2)*scale);}
            foreach(var edge in segments){using var pen=new Pen(edge.Color,edge.Arrow?2.2f:1.3f);if(edge.Arrow)pen.CustomEndCap=new AdjustableArrowCap(4,5);g.DrawLine(pen,Screen(edge.A),Screen(edge.B));}
            foreach(var point in scene.Nodes){var q=Screen(point);g.FillEllipse(Brushes.SteelBlue,q.X-2,q.Y-2,4,4);}
        }
        g.DrawString("Blue: geometry   Orange: input loads   Magenta: attributes   Red / green / blue: local axes",text,Brushes.DarkSlateGray,30,1032);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));bitmap.Save(path,ImageFormat.Png);Console.WriteLine("Preview gallery: "+Path.GetFullPath(path));
    }
}
