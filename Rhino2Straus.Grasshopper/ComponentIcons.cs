using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Rhino2Straus.Grasshopper;

/// <summary>Original, scalable glyphs, rendered at Grasshopper's native 24 px size.</summary>
public static class ComponentIcons
{
    private static readonly ConcurrentDictionary<string, Bitmap> Cache = new();
    public static Bitmap Get(string name, string category) => Cache.GetOrAdd(category + "/" + name, _ => Draw(name, category));

    private static Bitmap Draw(string name, string category)
    {
        string n = name.ToLowerInvariant(), c = category.ToLowerInvariant();
        bool load = category==ComponentTopics.Loads||c.Contains("load"), result = ComponentTopics.IsResult(category)||c.Contains("result"), section = n.Contains("section");
        Color accent = load ? Color.FromArgb(229, 114, 48) : result ? Color.FromArgb(153, 107, 202)
            : c.Contains("material") || section ? Color.FromArgb(72, 160, 131) : Color.FromArgb(44, 155, 194);
        var bitmap = new Bitmap(24, 24);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);
        using var ink = new Pen(Color.FromArgb(33, 53, 69), 1.65f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        using var highlight = new Pen(accent, 2) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        using var fill = new SolidBrush(Color.FromArgb(65, accent));
        using var solid = new SolidBrush(accent);
        void Line(float x, float y, float a, float b) => g.DrawLine(ink, x, y, a, b);
        void Dot(float x, float y) { g.FillEllipse(solid,x-2,y-2,4,4); g.DrawEllipse(ink,x-2,y-2,4,4); }
        void Arrow(float x,float y,float a,float b) { g.DrawLine(highlight,x,y,a,b); double angle=Math.Atan2(b-y,a-x); var p=new PointF(a,b); g.DrawLine(highlight,p,new PointF(a-3*(float)Math.Cos(angle-.6),b-3*(float)Math.Sin(angle-.6)));g.DrawLine(highlight,p,new PointF(a-3*(float)Math.Cos(angle+.6),b-3*(float)Math.Sin(angle+.6))); }
        void Plate() { var points=new[]{new PointF(3,15),new PointF(9,5),new PointF(21,8),new PointF(15,18)};g.FillPolygon(fill,points);g.DrawPolygon(ink,points); }
        void Cube(float x,float y,float s) { var p=new[]{new PointF(x,y+s*.4f),new PointF(x+s*.55f,y),new PointF(x+s,y+s*.3f),new PointF(x+s,y+s*.8f),new PointF(x+s*.45f,y+s),new PointF(x,y+s*.7f)};g.FillPolygon(fill,p);g.DrawPolygon(ink,p);Line(x,y+s*.4f,x+s*.45f,y+s*.6f);Line(x+s*.45f,y+s*.6f,x+s,y+s*.3f);Line(x+s*.45f,y+s*.6f,x+s*.45f,y+s); }
        if (n.Contains("display settings")) {for(int y=5;y<=19;y+=7){Line(3,y,21,y);g.FillRectangle(solid,y==12?6:14,y-2,4,4);} }
        else if (n.Contains("preview")||n.Contains("display manager")) { g.DrawBezier(ink,2,12,8,2,16,2,22,12);g.DrawBezier(ink,2,12,8,22,16,22,22,12);g.FillEllipse(solid,8,8,8,8);g.FillEllipse(Brushes.White,10,9,3,3); }
        else if (n.Contains("axes") || n.Contains("axis") || n.Contains("align") || n.Contains("orientation")) { Arrow(9,15,20,15);Arrow(9,15,9,3);Arrow(9,15,3,21); }
        else if (n.Contains("decompose") || n.Contains("deconstruct")) { Cube(1,8,9);Arrow(11,12,16,5);Arrow(11,12,16,19);Cube(16,1,6);Cube(16,16,6); }
        else if (n.Contains("clear")) { g.FillPolygon(fill,new[]{new PointF(3,15),new PointF(12,4),new PointF(21,11),new PointF(13,21)});g.DrawPolygon(ink,new[]{new PointF(3,15),new PointF(12,4),new PointF(21,11),new PointF(13,21)});Line(3,21,22,21); }
        else if (n.Contains("offset") || n.Contains("insertion")) { g.DrawRectangle(ink,4,7,9,13);g.DrawRectangle(highlight,10,2,9,13);Arrow(5,4,9,4); }
        else if (n.Contains("merge") || n.Contains("build model")) { Cube(1,3,9);Cube(1,13,9);Arrow(11,8,16,12);Arrow(11,17,16,13);Cube(15,9,7); }
        else if (n.Contains("export") || n.Contains("bake")) { Cube(2,9,12);Arrow(11,7,21,7);Line(18,3,21,3);Line(21,3,21,20);Line(18,20,21,20); }
        else if (n.Contains("read") || n.Contains("import")) { Cube(10,10,12);Arrow(2,7,12,7);Line(2,3,7,3);Line(2,3,2,19); }
        else if (n.Contains("run") || n.Contains("solver")) { g.DrawEllipse(ink,3,3,18,18);g.FillPolygon(solid,new[]{new PointF(9,7),new PointF(17,12),new PointF(9,17)}); }
        else if (n.Contains("validate")) { g.DrawRectangle(ink,3,3,16,18);g.DrawLines(highlight,new[]{new PointF(7,12),new PointF(11,16),new PointF(21,5)}); }
        else if (result || n.Contains("deformed") || n.Contains("envelope")) { Line(3,2,3,20);Line(3,20,22,20);g.DrawLines(highlight,new[]{new PointF(4,16),new PointF(8,12),new PointF(12,15),new PointF(17,5),new PointF(21,8)}); }
        else if (n.Contains("options")) { for(int y=5;y<=19;y+=7){Line(3,y,21,y);g.FillRectangle(solid,y==12?6:14,y-2,4,4);} }
        else if (n.Contains("rigid") && n.Contains("link")) { g.DrawLine(highlight,3,10,21,10);g.DrawLine(highlight,3,14,21,14);Dot(3,12);Dot(21,12); }
        else if (n.Contains("pinned") && n.Contains("link")) { Line(3,12,9,12);Line(15,12,21,12);g.DrawEllipse(highlight,9,9,6,6);Dot(3,12);Dot(21,12); }
        else if (n.Contains("masterslave")) { Dot(4,12);Dot(20,12);Arrow(8,8,17,8);Arrow(17,16,8,16); }
        else if (n.Contains("sectorsymmetry")) { g.DrawArc(highlight,3,3,18,18,190,140);Line(12,12,3,7);Line(12,12,20,5);Dot(3,7);Dot(20,5); }
        else if (n.Contains("spring") || n.Contains("link")) { g.DrawLines(ink,new[]{new PointF(2,12),new PointF(5,12),new PointF(7,6),new PointF(10,18),new PointF(13,6),new PointF(16,18),new PointF(18,12),new PointF(22,12)});Dot(2,12);Dot(22,12); }
        else if (n.Contains("support") || n.Contains("restraint") || n.Contains("prescribed")) { g.DrawPolygon(ink,new[]{new PointF(12,6),new PointF(5,17),new PointF(19,17)});Line(3,20,21,20);Dot(12,5); }
        else if (n.Contains("release")) { Line(2,16,10,12);Line(14,10,22,6);g.DrawEllipse(highlight,9,8,6,6); }
        else if (section)
        {
            string shape=n.Contains("circle")?"O":n.Contains("pipe")?"P":n.Contains("box")||n.Contains("rectangular hollow")?"B":n.Contains("channel")?"C":n.Contains("angle")?"L":n.Contains("tee")?"T":n.Contains("rect")?"R":n.Contains("generic")?"G":"I";
            using var bar=new Pen(accent,3.5f);
            if(n.Contains("triangle")) {g.DrawPolygon(bar,new[]{new PointF(4,4),new PointF(20,4),new PointF(12,21)});if(n.Contains("hollow"))g.DrawPolygon(ink,new[]{new PointF(8,7),new PointF(16,7),new PointF(12,15)});}
            else if(n.Contains("trapezoid")) {g.DrawPolygon(bar,new[]{new PointF(3,4),new PointF(21,4),new PointF(17,20),new PointF(7,20)});if(n.Contains("hollow"))g.DrawPolygon(ink,new[]{new PointF(7,8),new PointF(17,8),new PointF(15,16),new PointF(9,16)});}
            else if(n.Contains("cruciform")) {g.DrawLine(bar,12,3,12,21);g.DrawLine(bar,3,12,21,12);}
            else if(n.EndsWith(" z")) {g.DrawLines(bar,new[]{new PointF(3,4),new PointF(12,4),new PointF(12,20),new PointF(21,20)});}
            else if(n.Contains("top hat")) {g.DrawLines(bar,new[]{new PointF(2,20),new PointF(6,20),new PointF(6,5),new PointF(18,5),new PointF(18,20),new PointF(22,20)});}
            else if(n.Contains("bulb")) {g.DrawLine(bar,7,3,7,20);g.FillEllipse(solid,7,13,12,8);}
            else if(n.Contains("generic")||n.Contains("bxs")) {g.DrawPolygon(bar,new[]{new PointF(4,4),new PointF(18,3),new PointF(21,11),new PointF(12,10),new PointF(14,21),new PointF(4,19)});}
            else if(shape=="O"||shape=="P") {g.FillEllipse(fill,4,4,16,16);g.DrawEllipse(bar,4,4,16,16);if(shape=="P")g.DrawEllipse(ink,8,8,8,8);}
            else if(shape=="R"||shape=="B"){g.FillRectangle(fill,5,3,14,18);g.DrawRectangle(bar,5,3,14,18);if(shape=="B")g.DrawRectangle(ink,9,7,6,10);}
            else {if(shape!="L")g.DrawLine(bar,5,4,19,4);g.DrawLine(bar,shape is "C" or "L"?5:12,4,shape is "C" or "L"?5:12,20);if(shape!="T")g.DrawLine(bar,5,20,19,20);if(n.Contains("lipped")){g.DrawLine(bar,19,4,19,8);if(shape=="T")g.DrawLine(bar,5,4,5,8);else g.DrawLine(bar,19,16,19,20);}}
        }
        else if (load || n.Contains("temperature") || n.Contains("preload")) { if(n.Contains("plate")||n.Contains("area"))Plate();else Line(2,18,22,18);Arrow(6,3,6,13);Arrow(12,3,12,13);Arrow(18,3,18,13); }
        else if (n.Contains("plate") || n.Contains("area") || n.Contains("mesh") || n.Contains("thickness")) Plate();
        else if (n.Contains("beam") || n.Contains("frame") || n.Contains("polyline")) { Line(3,19,20,5);Dot(3,19);Dot(20,5);g.DrawLine(highlight,6,20,22,7); }
        else if (n.Contains("node")) { Line(3,18,12,11);Line(12,11,21,18);Line(12,11,12,3);Dot(12,11); }
        else if (n.Contains("solid")) Cube(3,3,18);
        else if (c.Contains("material")) {g.FillRectangle(fill,4,4,16,16);g.DrawRectangle(ink,4,4,16,16);Line(4,9,20,9);Line(4,15,20,15);Line(10,4,10,9);Line(14,9,14,15);Line(8,15,8,20);}
        else Cube(3,3,18);

        // A small semantic badge distinguishes related operations at normal canvas zoom.
        string badge=n.Contains("stress")?"σ":n.Contains("moment")?"M":n.Contains("force")?"F":n.Contains("mass")?"m":n.Contains("temperature")||n.Contains("gradient")?"T":n.Contains("info")||n.Contains("summary")?"i":n.Contains("filter")?"?":n.Contains("assign")?"+":n.Contains("units")?"u":n.Contains("combination")?"Σ":n.Contains("factor")?"×":n.Contains("case")?"C":n.Contains("displacement")?"δ":n.Contains("reactions")?"R":n.Contains("frequency")||n.Contains("modal")?"f":n.Contains("buckling")?"λ":"";
        if(badge.Length>0) {g.FillEllipse(Brushes.White,13,12,11,12);using var font=new Font("Segoe UI",8,FontStyle.Bold,GraphicsUnit.Point);using var brush=new SolidBrush(Color.FromArgb(33,53,69));g.DrawString(badge,font,brush,13,10.5f);}
        return bitmap;
    }
}
