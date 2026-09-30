using System;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

internal sealed partial class ExampleDocument
{
    public void First(GH_Component source,GH_Component target,string input) => Wire(source,source.Params.Output[0].Name,target,input);
    public GH_Component Step(string type,GH_Component source,string input,float x,float y)
    {var c=Component(type,x,y);First(source,c,input);return c;}
    public GH_Component Gate(IGH_Param source,GH_BooleanToggle toggle,float x,float y)
    {var gate=Add(new Grasshopper.Kernel.Components.GH_StreamGateComponent(),x,y);Wire(source,gate,"Stream");Wire(toggle,gate,"Gate");return gate;}
    public void Triggers(GH_Component component,float x,float y)
    {
        foreach(var p in component.Params.Input.Where(p=>new[]{"Run","Read","Bake","Overwrite","Overwrite results"}.Contains(p.Name)))
        {Wire(Toggle(p.Name+" / "+component.NickName,false,x,y),component,p.Name);y+=65;}
    }
    public Param_Mesh MeshInput(string name,Mesh mesh,float x,float y)
    {var p=Add(new Param_Mesh{NickName=name,Hidden=true},x,y);p.SetPersistentData(new GH_Mesh(mesh));return p;}
    public GH_Component Case(string name,float x,float y)
    {var c=Component("LoadCaseComponent",x,y);Set(c,"Name",name);return c;}
    public GH_Component BeamProperty(float x,float y)
    {
        var m=Component("DatabaseSteelMaterialComponent",x,y);var s=Component("FrameSectionRectangularComponent",x,y+150);
        var p=Component("FramePropertyComponent",x+340,y+70);First(m,p,"Material");First(s,p,"Section");return p;
    }
    public GH_Component PlateProperty(float x,float y)
    {
        var m=Component("DatabaseConcreteMaterialComponent",x,y);var s=Component("AreaThicknessComponent",x,y+150);
        var p=Component("AreaPropertyComponent",x+340,y+70);First(m,p,"Material");First(s,p,"Section");return p;
    }
    public GH_Component Beam(float x,float y,GH_Component property,Line? line=null)
    {
        var axis=Curves("Asse trave [m]",x,y,line??new Line(0,0,0,4,0,0));
        var b=Component("FrameElementComponent",x+300,y);Wire(axis,b,"Frame line");First(property,b,"Frame Property");return b;
    }
    public (GH_Component Model,GH_Component Beam,GH_Component Case) CantileverModel()
    {
        var prop=BeamProperty(30,310);var beam=Beam(30,660,prop);
        var support=FixedSupport(Points("Incastro",30,930,Point3d.Origin),330,930);
        var node=Component("NodeElementComponent",330,1200);Wire(Points("Nodo libero",30,1200,new Point3d(4,0,0)),node,"Node point");
        var lc=Case("Q",330,1400);var load=Step("LoadNodalComponent",node,"Node",820,1200);First(lc,load,"Load case");Set(load,"Fz",-10.0);
        var model=Component("BuildModelComponent",1270,770);First(beam,model,"Frame element");First(support,model,"Node");First(load,model,"Node");
        FinalModel=model;return(model,beam,lc);
    }
    public GH_Component StaticSolver(GH_Component model,float x,float y,string stem=null)
    {
        var solve=Component("RunModelCasesComponent",x,y);First(model,solve,"Model");
        Set(solve,"Model path",@"C:\Temp\Rhino2StrausExamples\"+(stem??FileName)+".st7");Triggers(solve,x-370,y+190);
        Readout(solve,"Status","Stato solver",x+340,y-40,360,80);return solve;
    }
    public GH_Component ResultGate(GH_Component solve,float x,float y)
    {
        var run=(GH_BooleanToggle)Input(solve,"Run").Sources.Single();return Gate(Output(solve,"Model"),run,x,y);
    }
    public void Finish(GH_Component model,float x=1740,float y=360)
    {FinalModel=model;Review(model,x,y);Preview(model,x,y+850);}
}
