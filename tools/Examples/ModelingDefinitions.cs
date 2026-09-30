using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

internal static partial class Definitions
{
    public static IEnumerable<Func<Assembly,ExampleDocument>> All() => new Func<Assembly,ExampleDocument>[]
    {
        Cantilever,Portal,Slab,Frames,Nodes,Links,MeshVariants,Solids,Sections,NativeStandard,NativeBgl,Bxs,
        ModelTools,BeamLoads,PlateLoads,Files,Modal,Buckling,Nonlinear,BeamResults,PlateSolidResults,FileResults,Reread
    };
    private static ExampleDocument New(Assembly p,string name,string description)
    {
        var e=new ExampleDocument(p,name){Description=description};
        e.Note(name.Replace('_',' '),description+"\nUnita': m, kN, kPa, tonne, C. Leggere gli input e i pannelli; valori dimostrativi modificabili.",30,20,1200,170);return e;
    }
    public static ExampleDocument Frames(Assembly p)
    {
        var e=New(p,"04_frame_polilinea_assi_svincoli","Una polilinea genera tre travi. Sezione con punto d'inserimento, asse locale verso un punto, svincolo torsionale finale e cambio proprieta'.\nBeam Behaviour permette Beam / Truss / Cable. Qui rimane Beam: Truss e Cable non sostengono flessione.");
        var prop=e.BeamProperty(30,300);var section=e.Document.Objects.OfType<GH_Component>().Single(c=>c.GetType().Name=="FrameSectionRectangularComponent");
        var insert=e.Step("SectionInsertionComponent",section,"Section",710,430);e.Set(insert,"Insertion",1);ExampleDocument.Input(prop,"Section").RemoveAllSources();e.First(insert,prop,"Section");
        var behaviour=e.Step("BeamBehaviourComponent",prop,"Property",1040,340);
        var curve=e.Add(new Param_Curve{NickName="Polilinea 3 segmenti",Hidden=true},30,790);curve.SetPersistentData(new GH_Curve(new PolylineCurve(new[]{new Point3d(0,0,0),new Point3d(3,0,0),new Point3d(3,0,2),new Point3d(6,0,2)})));
        var beams=e.Component("PolylineBeamsComponent",340,790);e.Wire(curve,beams,"Polyline");e.First(behaviour,beams,"Property");e.Set(beams,"First ID",10);
        var assigned=e.Step("BeamAssignPropertyComponent",beams,"Beam",680,790);e.First(behaviour,assigned,"Property");
        var align=e.Step("AlignBeamToPointComponent",assigned,"Beam",1030,790);e.Set(align,"Target",new Point3d(1,5,4));
        var release=e.Step("BeamReleasesComponent",align,"Beam",1370,790);e.Set(release,"Start",new bool[6]);e.Set(release,"End",new[]{false,false,false,false,false,true});
        var axes=e.Step("BeamAxesComponent",release,"Element",1370,1080);e.Readout(axes,"Axis 1","Direzione asse 1",1700,1050);
        var info=e.Step("BeamInfoComponent",release,"Beam",1370,1300);e.Readout(info,"ID","ID travi 10, 11, 12",1700,1310);
        var fix=e.FixedSupport(e.Points("Base",30,1110,Point3d.Origin),340,1110);
        var model=e.Component("BuildModelComponent",1770,740);e.First(release,model,"Frame element");e.First(fix,model,"Node");
        e.ExpectedCounts=new[]{4,3,0,0,0};e.Finish(model,2130,350);return e;
    }
    public static ExampleDocument Nodes(Assembly p)
    {
        var e=New(p,"05_nodi_vincoli_molle_masse_temperatura","Attributi in sequenza sullo stesso nodo: cedimento imposto, molle, masse, temperatura e forza.\nBuild riceve solo l'ultima copia: collegare copie intermedie sommerebbe nuovamente i contributi. Le rotazioni sono in radianti.");
        var prop=e.BeamProperty(30,300);var beam=e.Beam(30,660,prop);
        var node=e.Component("NodeElementComponent",330,970);e.Wire(e.Points("Nodo base",30,970,Point3d.Origin),node,"Node point");
        var displacement=e.Step("PrescribedDisplacementComponent",node,"Node",650,970);e.Set(displacement,"Restrained",Enumerable.Repeat(true,6).ToArray());e.Set(displacement,"Values",new[]{0d,0,-.005,0,0,0});
        var spring=e.Step("NodeSpringComponent",displacement,"Node",970,970);e.Set(spring,"Translation",new Vector3d(1000,1000,1000));
        var mass=e.Step("NodeMassComponent",spring,"Node",1290,970);e.Set(mass,"Translation",new Vector3d(.1,.1,.1));e.Set(mass,"Rotation",new Vector3d(.01,.01,.01));
        var lc=e.Case("T",970,1280);e.Set(lc,"Reference temperature",15.0);var temp=e.Step("NodeTemperatureComponent",mass,"Node",1610,970);e.First(lc,temp,"Load case");e.Set(temp,"Temperature",20.0);
        var info=e.Step("NodeInfoComponent",temp,"Node",1920,1060);e.Readout(info,"Restraints","Sei gradi di liberta'",2260,1060);
        var model=e.Component("BuildModelComponent",1920,650);e.First(beam,model,"Frame element");e.First(temp,model,"Node");e.ExpectedCounts=new[]{2,1,0,1,0};e.Finish(model,2300,350);return e;
    }
    public static ExampleDocument Links(Assembly p)
    {
        var e=New(p,"06_link_rigidi_elastici_master_slave","Confronto dei cinque tipi: rigid, pinned, master-slave, sector symmetry e connessione elastica.\nLe opzioni controllano piano rigido, asse di simmetria e sei relazioni DoF (libero / uguale / opposto).\nLe rigidezze elastiche sono negli assi globali. Ogni coppia ha il primo nodo bloccato. Esempio di assegnazione: non ogni configurazione e' stabile per ogni solver.");
        var options=e.Component("LinkConstraintOptionsComponent",30,380);
        var types=new[]{"RigidLinkPropertyComponent","PinnedLinkPropertyComponent","MasterSlaveLinkPropertyComponent","SectorSymmetryLinkPropertyComponent","ElasticLinkPropertyComponent","LinkPropertyComponent"};
        var model=e.Component("BuildModelComponent",1540,800);
        for(int i=0;i<types.Length;i++)
        {
            float y=650+i*370;var property=e.Component(types[i],340,y);
            if(property.Params.Input.Any(v=>v.Name=="Options"))e.First(options,property,"Options");
            if(types[i].Contains("Elastic")||types[i]=="LinkPropertyComponent")
            {if(types[i]=="LinkPropertyComponent")e.Set(property,"Type",1);foreach(string k in new[]{"Kx","Ky","Kz","Rx","Ry","Rz"})e.Set(property,k,1000.0);}
            var link=e.Component("LinkElementComponent",960,y);e.First(property,link,"Link Property");
            e.Set(link,"Start Point",new Point3d(0,i*2,0));e.Set(link,"End Point",new Point3d(1,i*2,0));e.Set(link,"ID",i+1);e.First(link,model,"Link element");
        }
        var points=e.Points("Basi link",30,3060,Enumerable.Range(0,6).Select(i=>new Point3d(0,i*2,0)).ToArray());var fix=e.FixedSupport(points,340,3060);e.First(fix,model,"Node");
        e.ExpectedCounts=new[]{12,0,0,0,0};e.ExpectedLinks=6;e.ExpectedVolume=0;e.Finish(model,1900,450);return e;
    }
    private static Mesh PanelMesh(double x,bool split=false)
    {
        var m=new Mesh();m.Vertices.Add(x,0,0);m.Vertices.Add(x+2,0,0);m.Vertices.Add(x+2,2,0);m.Vertices.Add(x,2,0);
        if(split){m.Faces.AddFace(0,1,2);m.Faces.AddFace(0,2,3);}else m.Faces.AddFace(0,1,2,3);
        m.Normals.ComputeNormals();return m;
    }
    public static ExampleDocument MeshVariants(Assembly p)
    {
        var e=New(p,"07_mesh_tri_quad_proprieta_assi","Una mesh mista produce due Tri3 e un Quad4; un'altra faccia usa Area Element.\nRiassegnazione della proprieta', allineamento a un piano e lettura degli assi. Invertire l'ordine dei vertici cambia normale e lato dei carichi.");
        var prop=e.PlateProperty(30,300);var m=PanelMesh(0,true);m.Append(PanelMesh(3));
        var input=e.MeshInput("Mesh mista",m,30,720);var plates=e.Component("MeshPlatesComponent",350,720);e.Wire(input,plates,"Mesh");e.First(prop,plates,"Property");
        var assigned=e.Step("PlateAssignPropertyComponent",plates,"Plate",690,720);e.First(prop,assigned,"Property");
        var align=e.Step("AlignPlateToPlaneComponent",assigned,"Plate",1020,720);e.Set(align,"Reference",new Plane(Point3d.Origin,new Vector3d(1,1,0),new Vector3d(-1,1,0)));
        var axes=e.Step("PlateAxesComponent",align,"Element",1360,1000);e.Readout(axes,"Axis 1","Assi locali 1",1700,1050);
        var info=e.Step("PlateInfoComponent",align,"Plate",1360,1230);e.Readout(info,"ID","ID delle piastre",1700,1300);
        var one=e.Component("AreaElementComponent",690,1620);e.Wire(e.MeshInput("Faccia singola",PanelMesh(6),30,1620),one,"Area");e.First(prop,one,"Area Property");
        var model=e.Component("BuildModelComponent",1700,700);e.First(align,model,"Area element");e.First(one,model,"Area element");
        e.ExpectedCounts=new[]{12,0,4,0,0};e.Finish(model,2080,360);return e;
    }
    private static Point3d[][] SolidPoints() => new[]{
        new[]{new Point3d(0,0,0),new Point3d(1,0,0),new Point3d(0,1,0),new Point3d(0,0,1)},
        new[]{new Point3d(2,0,0),new Point3d(3,0,0),new Point3d(2,1,0),new Point3d(2,0,1),new Point3d(3,0,1),new Point3d(2,1,1)},
        new[]{new Point3d(4,0,0),new Point3d(5,0,0),new Point3d(5,1,0),new Point3d(4,1,0),new Point3d(4,0,1),new Point3d(5,0,1),new Point3d(5,1,1),new Point3d(4,1,1)}};
    public static ExampleDocument Solids(Assembly p)
    {
        var e=New(p,"08_solidi_tet_wedge_hexa_pressione","Tet4, Wedge6 e Hexa8 costruiti con nodi ordinati secondo Straus.\nPressione su una faccia, incastro dei nodi a Z=0, controllo dei volumi e Brep chiusi per bake.\nVolumi attesi: 1/6, 1/2 e 1 m3. Numerazione delle facce nativa: controllare la preview prima del calcolo.");
        var material=e.Component("DatabaseConcreteMaterialComponent",30,330);var lc=e.Case("P_solidi",30,530);var model=e.Component("BuildModelComponent",1710,700);
        int index=0;foreach(var points in SolidPoints())
        {
            float y=830+index*400;var nodes=e.Component("NodeElementComponent",340,y);e.Wire(e.Points("Nodi ordinati "+points.Length,30,y,points),nodes,"Node point");
            var solid=e.Step("SolidElementComponent",nodes,"Nodes",670,y);e.First(material,solid,"Material");e.Set(solid,"ID",++index);
            var pressure=e.Step("SolidPressureComponent",solid,"Solid",1010,y);e.First(lc,pressure,"Load case");e.Set(pressure,"Face",1);e.Set(pressure,"Pressure",5.0);e.First(pressure,model,"Solids");
            var info=e.Step("SolidInfoComponent",pressure,"Solid",1340,y+100);e.Readout(info,"Volume","Volume [m3]",1630,y+150,210,80);
        }
        var fix=e.FixedSupport(e.Points("Nodi inferiori",30,2210,SolidPoints().SelectMany(x=>x).Where(n=>n.Z==0).ToArray()),340,2210);e.First(fix,model,"Node");
        e.ExpectedCounts=new[]{18,0,0,1,0};e.ExpectedSolids=3;e.ExpectedVolume=5.0/3;e.Finish(model,2100,380);return e;
    }
    public static ExampleDocument Sections(Assembly p)
    {
        var e=New(p,"09_materiali_sezioni_base_generica","Otto tipologie geometriche e una sezione generica con contorno esplicito per la preview.\nMaterial custom con resistenze nominali assegnate separatamente (MPa). Le resistenze non modificano il modulo elastico.\nLa sezione generica e' un quadrato 0.1 x 0.1 m: i valori numerici e il contorno devono essere coerenti; il profilo non ricalcola la rigidezza.");
        var material=e.Component("MaterialComponent",30,350);e.Set(material,"Name","Acciaio custom");
        var strengths=e.Step("MaterialStrengthsComponent",material,"Material",370,350);e.Set(strengths,"fy",355.0);e.Set(strengths,"fu",510.0);
        var mi=e.Step("MaterialInfoComponent",strengths,"Material",720,330);e.Readout(mi,"fy","fy [MPa]",1060,340);
        var names=new[]{"Rectangular","Circle","Pipe","I","Tee","Angle","Channel","Box","Generic"};var model=e.Component("BuildModelComponent",1900,930);
        for(int i=0;i<names.Length;i++)
        {
            float y=800+i*440;var section=e.Component("FrameSection"+names[i]+"Component",340,y);e.Set(section,"Name",names[i]);
            if(names[i]=="Generic")e.Set(section,"Profile",Brep.CreatePlanarBreps(new Rectangle3d(Plane.WorldXY,new Interval(-.05,.05),new Interval(-.05,.05)).ToNurbsCurve(),1e-8).Single());
            var property=e.Component("FramePropertyComponent",760,y);e.Set(property,"Name",names[i]);e.First(section,property,"Section");e.First(strengths,property,"Material");
            var beam=e.Beam(900,y,property,new Line(0,i,0,2,i,0));e.First(beam,model,"Frame element");
            var info=e.Step("SectionInfoComponent",section,"Section",760,y+200);e.Readout(info,"Area","Area [m2]",1100,y+240,220,75);
        }
        e.ExpectedCounts=new[]{18,9,0,0,0};e.Finish(model,2300,400);return e;
    }
    public static ExampleDocument ModelTools(Assembly p)
    {
        var e=New(p,"13_modello_gruppi_filtri_ricostruzione","Gruppi gerarchici, filtri per posizione/proprieta'/gruppo, decomposizione e ricostruzione.\nI filtri combinano i criteri con OR. Un solo Model finale. Per un import collegare anche Preserve nodes e Self weight.\nMerge salda le parti create; se una parte viene da import, conserva i nodi distinti.");
        var f=e.CantileverModel();var group=e.Component("ElementGroupComponent",30,1610);e.Set(group,"Group Name","Edificio");
        var child=e.Component("ElementGroupComponent",350,1610);e.Set(child,"Group Name","Travi");e.First(group,child,"Group Parent");e.First(child,f.Beam,"Groups");
        var d=e.Step("DecomposeModelComponent",f.Model,"Model",1650,740);
        var bf=e.Component("FrameFilterComponent",2020,700);e.Wire(d,"Frame elements",bf,"Frame elements");e.First(child,bf,"Groups");
        var nf=e.Component("NodeFilterComponent",2020,1000);e.Wire(d,"Node elements",nf,"Node elements");e.Set(nf,"Position",Point3d.Origin);
        var rebuild=e.Component("BuildModelComponent",2410,770);e.Wire(d,"Units",rebuild,"Unit");e.Wire(d,"Preserve nodes",rebuild,"Preserve nodes");e.Wire(d,"Self weight",rebuild,"Self weight");
        e.Wire(bf,"Filtered Frames",rebuild,"Frame element");e.Wire(bf,"Other Frames",rebuild,"Frame element");e.Wire(nf,"Filtered Nodes",rebuild,"Node");e.Wire(nf,"Other Nodes",rebuild,"Node");
        e.Wire(d,"Combinations",rebuild,"Load combination");e.Wire(d,"Load cases",rebuild,"Load cases");
        var aligned=e.Step("AlignModelAxesComponent",rebuild,"Model",2770,760);e.Set(aligned,"Direction",Vector3d.ZAxis);
        var clear=e.Step("ClearModelResultsComponent",aligned,"Model",3100,760);e.ExpectedCounts=new[]{2,1,0,1,0};e.Finish(clear,3490,350);return e;
    }
    public static ExampleDocument BeamLoads(Assembly p)
    {
        var e=New(p,"14_carichi_travi_peso_proprio","Carichi concatenati: forza e momento concentrati, distribuzione lineare parziale, gradiente termico e pretensione.\nPosizioni relative da nodo 1: 0..1. Carichi locali lungo assi principali; carichi globali indipendenti dalla rotazione della sezione.\nPeso proprio G separato dai carichi Q. Tutti gli attributi rimangono nel Model unico.");
        var f=e.CantileverModel();GH_Component current=f.Beam;float x=1660;
        foreach(var type in new[]{"BeamPointForceComponent","BeamPointMomentComponent","LoadFrameNonUniComponent","BeamGradientComponent","BeamPreloadComponent"})
        {
            string input=type=="LoadFrameNonUniComponent"?"Frame":"Beam";var c=e.Step(type,current,input,x,760);e.First(f.Case,c,"Load case");
            if(type=="BeamPointForceComponent")e.Set(c,"Value",new Vector3d(0,0,-5));
            if(type=="BeamPointMomentComponent")e.Set(c,"Value",new Vector3d(0,2,0));
            if(type=="LoadFrameNonUniComponent"){e.Set(c,"Load at start",-1.0);e.Set(c,"Load at end",-4.0);e.Set(c,"Location relative start",.2);e.Set(c,"Location relative end",.8);}
            if(type=="BeamGradientComponent"){e.Set(c,"Gradient 1",10.0);e.Set(c,"Gradient 2",5.0);}
            if(type=="BeamPreloadComponent")e.Set(c,"Value",10.0);
            current=c;x+=420;
        }
        ExampleDocument.Input(f.Model,"Frame element").RemoveAllSources();e.First(current,f.Model,"Frame element");
        var g=e.Case("G",1650,1450);var sw=e.Step("SelfWeightComponent",g,"Load case",2030,1450);e.First(sw,f.Model,"Self weight");
        e.ExpectedCounts=new[]{2,1,0,2,0};e.Finish(f.Model,3870,350);return e;
    }
    public static ExampleDocument PlateLoads(Assembly p)
    {
        var e=New(p,"15_carichi_piastre_termici_precarico","Carico normale alla piastra, pressione globale, pressione su bordo, gradiente termico e predeformazione nel piano.\nI carichi vengono applicati in sequenza sulla stessa piastra. Il filtro finale mostra la selezione per proprieta'.\nNormal pressure e Global pressure sono attributi diversi: qui vengono sommati intenzionalmente per illustrare il flusso.");
        var prop=e.PlateProperty(30,320);var plate=e.Component("AreaElementComponent",360,800);e.Wire(e.MeshInput("Quad4",PanelMesh(0),30,800),plate,"Area");e.First(prop,plate,"Area Property");var lc=e.Case("Q",30,1100);
        GH_Component current=plate;float x=760;
        foreach(var type in new[]{"LoadAreaUniformComponent","GlobalPressureComponent","PlateEdgePressureComponent","PlateGradientComponent","PlatePreloadComponent"})
        {
            var c=e.Step(type,current,type=="LoadAreaUniformComponent"?"Area":"Plate",x,800);e.First(lc,c,"Load case");
            if(type=="LoadAreaUniformComponent"){e.Set(c,"Direction",2);e.Set(c,"Load",2.0);}
            if(type=="GlobalPressureComponent")e.Set(c,"Pressure",new Vector3d(0,0,-3));
            if(type=="PlateEdgePressureComponent")e.Set(c,"Pressure",1.0);
            if(type=="PlateGradientComponent")e.Set(c,"Gradient",10.0);
            if(type=="PlatePreloadComponent"){e.Set(c,"Values",new Vector3d(.0001,.0001,0));e.Set(c,"Strain",true);}
            current=c;x+=380;
        }
        var filter=e.Step("AreaFilterComponent",current,"Area elements",2710,800);
        e.First(prop,filter,"Area Properties");var model=e.Component("BuildModelComponent",3080,800);e.Wire(filter,"Filtered Areas",model,"Area element");
        var fix=e.FixedSupport(e.Points("Quattro appoggi",30,1590,PanelMesh(0).Vertices.ToPoint3dArray()),350,1590);e.First(fix,model,"Node");
        e.ExpectedCounts=new[]{4,0,1,1,0};e.Finish(model,3490,340);return e;
    }
}
