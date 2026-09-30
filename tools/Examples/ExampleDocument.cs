using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

internal sealed partial class ExampleDocument : IDisposable
{
    private readonly Assembly plugin;
    public GH_Document Document { get; } = new();
    public string FileName { get; }
    public GH_Component FinalModel { get; set; }
    public string Description { get; set; } = "";
    public int? ExpectedSolids { get; set; }
    public int? ExpectedLinks { get; set; }
    public int[] ExpectedCounts { get; set; }
    public double? ExpectedVolume { get; set; }
    public ExampleDocument(Assembly assembly, string fileName)
    {
        plugin=assembly; FileName=fileName;
        Document.Properties.Description="Esempio autonomo Rhino2Straus. Geometrie interne; metri e kN. Leggere le note sul canvas e examples/README.md.";
    }
    public void Dispose()=>Document.Dispose();
    public T Add<T>(T obj, float x, float y) where T:IGH_DocumentObject
    {
        obj.CreateAttributes(); obj.Attributes.Pivot=new PointF(x,y);
        Document.AddObject(obj,false);
        return obj;
    }
    public GH_Component Component(string className,float x,float y)
    {
        var type=plugin.GetTypes().Single(t=>t.Name==className);
        return Add((GH_Component)Activator.CreateInstance(type),x,y);
    }
    public static IGH_Param Input(GH_Component c,string name)=>c.Params.Input.Single(p=>p.Name==name);
    public static IGH_Param Output(GH_Component c,string name)=>c.Params.Output.Single(p=>p.Name==name);
    public void Wire(GH_Component source,string output,GH_Component target,string input)=>Input(target,input).AddSource(Output(source,output));
    public void Wire(IGH_Param source,GH_Component target,string input)=>Input(target,input).AddSource(source);
    public void Set(GH_Component c,string input,object value)
    {
        var p=Input(c,input);
        switch(p)
        {
            case Param_Number n when value is double[] values: n.PersistentData.Clear(); n.SetPersistentData(values.Select(v=>new GH_Number(v))); break;
            case Param_Boolean b when value is bool[] values: b.PersistentData.Clear(); b.SetPersistentData(values.Select(v=>new GH_Boolean(v))); break;
            case Param_Point point: point.PersistentData.Clear(); point.SetPersistentData(new GH_Point((Point3d)value)); break;
            case Param_Plane plane: plane.PersistentData.Clear(); plane.SetPersistentData(new GH_Plane((Plane)value)); break;
            case Param_Brep brep: brep.PersistentData.Clear(); brep.SetPersistentData(new GH_Brep((Brep)value)); break;
            case Param_Number n: n.PersistentData.Clear(); n.SetPersistentData(new GH_Number(Convert.ToDouble(value))); break;
            case Param_Integer n: n.PersistentData.Clear(); n.SetPersistentData(new GH_Integer(Convert.ToInt32(value))); break;
            case Param_Boolean b: b.PersistentData.Clear(); b.SetPersistentData(new GH_Boolean((bool)value)); break;
            case Param_String s: s.PersistentData.Clear(); s.SetPersistentData(new GH_String((string)value)); break;
            case Param_Vector v: v.PersistentData.Clear(); v.SetPersistentData(new GH_Vector((Vector3d)value)); break;
            default: throw new InvalidOperationException("Unsupported input "+c.Name+" / "+input);
        }
    }
    public GH_NumberSlider Slider(string name,decimal value,decimal min,decimal max,float x,float y,int places=2)
    {
        var slider=Add(new GH_NumberSlider{NickName=name},x,y);
        slider.Slider.Minimum=min; slider.Slider.Maximum=max; slider.Slider.DecimalPlaces=places;
        slider.SetSliderValue(value); return slider;
    }
    public GH_BooleanToggle Toggle(string name,bool value,float x,float y)=>Add(new GH_BooleanToggle{NickName=name,Value=value},x,y);
    public GH_Panel Note(string title,string text,float x,float y,float width=420,float height=140)
    {
        var panel=Add(new GH_Panel{NickName=title,UserText=text},x,y);
        panel.Properties.Colour=Color.FromArgb(255,249,221);
        panel.Properties.Font=new Font("Segoe UI",8);
        panel.Properties.Alignment=GH_Panel.Alignment.Left;
        panel.Properties.Wrap=true;
        panel.Properties.DrawPaths=false;panel.Properties.DrawIndices=false;
        int rows=text.Split('\n').Sum(line=>Math.Max(1,(int)Math.Ceiling(line.Length*7.0/(width-24))));
        height=Math.Max(height,rows*24+32);
        panel.Attributes.Bounds=new RectangleF(x,y,width,height);
        return panel;
    }
    public GH_Panel Readout(GH_Component source,string output,string title,float x,float y,float width=210,float height=100)
    {
        var panel=Note(title,"",x,y,width,height);
        panel.AddSource(Output(source,output));
        panel.Properties.Colour=Color.FromArgb(228,240,244);
        return panel;
    }
    public Param_Curve Curves(string name,float x,float y,params Line[] lines)
    {
        var p=Add(new Param_Curve{NickName=name,Hidden=true},x,y);
        p.SetPersistentData(lines.Select(l=>new GH_Curve(new LineCurve(l)))); return p;
    }
    public Param_Point Points(string name,float x,float y,params Point3d[] points)
    {
        var p=Add(new Param_Point{NickName=name,Hidden=true},x,y);
        p.SetPersistentData(points.Select(pt=>new GH_Point(pt)));return p;
    }
    public Param_Vector Vector(string name,Vector3d value,float x,float y)
    {
        var p=Add(new Param_Vector{NickName=name},x,y);p.SetPersistentData(new GH_Vector(value));return p;
    }
    public GH_Component FixedSupport(Param_Point points,float x,float y)
    {
        var node=Component("NodeElementComponent",x,y);Wire(points,node,"Node point");
        var fix=Component("NodeSupportComponent",x+230,y);Wire(node,"Node",fix,"Node");
        foreach(var dof in new[]{"Dx","Dy","Dz","Mx","My","Mz"})Set(fix,dof,true);
        return fix;
    }
    public void Group(string title,Color color,params IGH_DocumentObject[] objects)
    {
        var group=Add(new GH_Group{NickName=title,Colour=Color.FromArgb(65,color),Border=GH_GroupBorder.Box},0,0);
        foreach(var obj in objects)group.AddObject(obj.InstanceGuid);
    }
    public void GroupSince(int index,string title,Color color)=>Group(title,color,Document.Objects.Skip(index).Where(o=>o is not GH_Group).ToArray());
    public int Mark()=>Document.ObjectCount;
    public GH_Component Units(float x,float y)=>Component("ModelUnitsComponent",x,y);
    public void Review(GH_Component model,float x,float y)
    {
        int mark=Mark();
        var validate=Component("ValidateModelComponent",x,y);Wire(model,"Model",validate,"Model");
        Readout(validate,"Valid","Preflight valido",x+220,y-30,190,60);
        Readout(validate,"Errors","Errori",x+220,y+50,260,80);
        var summary=Component("ModelSummaryComponent",x,y+200);Wire(model,"Model",summary,"Model");
        Readout(summary,"Nodes","Numero nodi",x+220,y+170,115,70);
        Readout(summary,"Beams","Travi",x+345,y+170,100,70);
        Readout(summary,"Plates","Piastre",x+455,y+170,100,70);
        var decompose=Component("DecomposeModelComponent",x,y+440);Wire(model,"Model",decompose,"Model");
        GroupSince(mark,"Controlla e decomponi il Model",Color.SteelBlue);
    }
    public void Preview(GH_Component model,float x,float y,bool physicalComponent=false,bool results=false)
    {
        int mark=Mark();
        Note("Preview e bake","Preview ON/OFF spegne la visualizzazione gestita.\nDisplay Settings controlla sezioni, carichi, vincoli, offset e assi.\nBrep pronti: tasto destro > Bake. Il bake e' manuale.\nLe geometrie FEM e il Model restano disponibili anche con preview OFF.",x-140,y-220,660,130);
        var enabled=Toggle("Preview ON/OFF",true,x-130,y-40);
        var settings=Component("DisplaySettingsComponent",x,y+230);
        Set(settings,"Axes",true);Set(settings,"Symbol scale",.3);Set(settings,"Load scale",.06);
        Set(settings,"Values",false);Set(settings,"Results",results);Wire(enabled,settings,"Enabled");
        var preview=Component("PreviewModelComponent",x+400,y+110);
        Wire(model,"Model",preview,"Model");Wire(settings,"Settings",preview,"Settings");
        // Explicit master wire keeps the canvas legible and the same switch controls both inputs.
        Wire(enabled,preview,"Enabled");
        var brep=Add(new Param_Brep{NickName="Brep pronti per Bake",Hidden=true},x+730,y+110);
        brep.AddSource(Output(preview,"Breps"));
        Readout(preview,"Brep issues","Problemi Brep",x+650,y+210,230,90);
        if(physicalComponent)
        {
            var geo=Component("ModelGeometryComponent",x+400,y+430);geo.Hidden=true;
            Wire(model,"Model",geo,"Model");
            Note("Geometrie alternative","Model Physical Geometry espone anche linee d'asse e mesh.\nPer il bake solido usare l'uscita Breps o il menu del componente.",x+260,y+540,580,85);
        }
        GroupSince(mark,"Preview / sezioni / carichi / attributi / Brep",Color.MediumSeaGreen);
    }
}
