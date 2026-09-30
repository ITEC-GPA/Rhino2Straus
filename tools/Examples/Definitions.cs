using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

internal static partial class Definitions
{
    public static ExampleDocument Cantilever(Assembly plugin)
    {
        var e=new ExampleDocument(plugin,"01_mensola_offset_bake"){ExpectedCounts=new[]{2,1,0,1,0},ExpectedVolume=.24};
        e.Note("01 | Mensola / m / kN","1. Sezione rettangolare e acciaio S355.\n2. Trave di 4 m, incastro e forza Q=-10 kN.\n3. Offset locale di 0.05 m e asse 1 allineato a Z.\n4. Un solo Model; preview disattivabile e Brep chiuso per il bake.\nModifica gli slider e osserva sezioni e carichi. Nessuna API Straus richiesta.",30,20,790,145);
        int mark=e.Mark();
        var steel=e.Component("DatabaseSteelMaterialComponent",330,270);
        var h=e.Slider("Altezza [m]",.30m,.10m,.70m,30,370);
        var w=e.Slider("Larghezza [m]",.20m,.05m,.50m,30,430);
        var section=e.Component("FrameSectionRectangularComponent",330,410);e.Set(section,"Name","R parametrica");
        e.Wire(h,section,"Height");e.Wire(w,section,"Width");
        var property=e.Component("FramePropertyComponent",660,310);e.Set(property,"Name","Mensola S355");
        e.Wire(steel,"Steel Material",property,"Material");e.Wire(section,"Frame Section",property,"Section");
        e.GroupSince(mark,"01 | Materiale e sezione parametrica",Color.CornflowerBlue);
        mark=e.Mark();
        var line=e.Curves("Asse di 4 m",30,670,new Line(Point3d.Origin,new Point3d(4,0,0)));
        var beam=e.Component("FrameElementComponent",330,670);e.Wire(line,beam,"Frame line");e.Wire(property,"Frame Property",beam,"Frame Property");
        var offset=e.Component("BeamOffsetComponent",660,670);e.Set(offset,"Offset",new Vector3d(.05,0,0));e.Wire(beam,"Frame element",offset,"Beam");
        var direction=e.Vector("Asse 1 verso Z",Vector3d.ZAxis,650,850);
        var align=e.Component("AlignBeamAxisComponent",970,670);e.Wire(offset,"Beam",align,"Beam");e.Wire(direction,align,"Direction");
        e.Note("Offset prima dell'allineamento","Offset: X e Y sono lungo gli assi principali 1 e 2; Z=0.\nBeam Offset imposta anche l'angolo: applicare Align Beam Axis dopo.\nNon cambiare solo la preview: questi attributi sono nel Model.",30,830,530,110);
        e.GroupSince(mark,"02 | Elemento / offset / assi locali",Color.MediumPurple);
        mark=e.Mark();
        var basePoint=e.Points("Incastro",30,1110,Point3d.Origin);var support=e.FixedSupport(basePoint,330,1110);
        var end=e.Points("Estremo libero",30,1330,new Point3d(4,0,0));var node=e.Component("NodeElementComponent",330,1330);e.Wire(end,node,"Node point");
        var lc=e.Component("LoadCaseComponent",330,1510);e.Set(lc,"Name","Q");
        var force=e.Slider("Fz [kN]",-10m,-30m,0m,600,1510,1);
        var load=e.Component("LoadNodalComponent",970,1320);e.Wire(node,"Node",load,"Node");e.Wire(lc,"Load case",load,"Load case");e.Wire(force,load,"Fz");
        e.GroupSince(mark,"03 | Incastro e carico nodale Q",Color.SandyBrown);
        var units=e.Units(1300,380);
        var model=e.Component("BuildModelComponent",1300,710);e.Wire(units,"Model Units",model,"Unit");
        e.Wire(align,"Beam",model,"Frame element");e.Wire(support,"Node",model,"Node");e.Wire(load,"Node",model,"Node");e.FinalModel=model;
        e.Group("04 | Un solo Model",Color.SteelBlue,units,model);
        e.Review(model,1650,400);e.Preview(model,1650,1240,true);
        return e;
    }
    public static ExampleDocument Portal(Assembly plugin)
    {
        var e=new ExampleDocument(plugin,"02_portale_casi_combo_risultati"){ExpectedCounts=new[]{4,3,0,2,2},ExpectedVolume=.0816};
        e.Note("02 | Portale / m / kN","Due parti indipendenti confluiscono in Merge Straus Models con Renumber=true.\nPortale 6 x 3 m, S355, profilo I parametrico, incastri alla base.\nG=-5 e Q=-3 kN/m sulla trave. Due combinazioni dimostrative.\nRun=false: puoi esplorare il Model e la preview senza Straus7.\nLa zona Analisi mostra il percorso per ottenere lo stesso Model con risultati.",30,20,900,150);
        int mark=e.Mark();
        var steel=e.Component("DatabaseSteelMaterialComponent",300,270);
        var h=e.Slider("Altezza I [m]",.30m,.15m,.60m,30,460);
        var section=e.Component("FrameSectionIComponent",340,490);e.Set(section,"Name","I dimostrativa");e.Wire(h,section,"Height");
        var property=e.Component("FramePropertyComponent",670,330);e.Set(property,"Name","Portale S355");e.Wire(steel,"Steel Material",property,"Material");e.Wire(section,"Frame Section",property,"Section");
        e.GroupSince(mark,"01 | Materiale e sezione I",Color.CornflowerBlue);
        mark=e.Mark();
        var columns=e.Curves("Due pilastri",30,810,new Line(0,0,0,0,0,3),new Line(6,0,3,6,0,0));
        var col=e.Component("FrameElementComponent",340,810);e.Wire(columns,col,"Frame line");e.Wire(property,"Frame Property",col,"Frame Property");
        var alignCols=e.Component("AlignBeamAxisComponent",680,810);e.Set(alignCols,"Direction",Vector3d.XAxis);e.Wire(col,"Frame element",alignCols,"Beam");
        var bases=e.Points("Basi",30,1020,new Point3d(0,0,0),new Point3d(6,0,0));var support=e.FixedSupport(bases,340,1020);
        var modelA=e.Component("BuildModelComponent",1080,870);e.Wire(alignCols,"Beam",modelA,"Frame element");e.Wire(support,"Node",modelA,"Node");
        e.GroupSince(mark,"02 | Model A / pilastri e incastri",Color.MediumPurple);
        mark=e.Mark();
        var roof=e.Curves("Trave di 6 m",30,1280,new Line(0,0,3,6,0,3));
        var beam=e.Component("FrameElementComponent",340,1280);e.Wire(roof,beam,"Frame line");e.Wire(property,"Frame Property",beam,"Frame Property");
        var align=e.Component("AlignBeamAxisComponent",680,1280);e.Set(align,"Direction",Vector3d.ZAxis);e.Wire(beam,"Frame element",align,"Beam");
        var g=e.Component("LoadCaseComponent",340,1500);e.Set(g,"Name","G");
        var q=e.Component("LoadCaseComponent",340,1660);e.Set(q,"Name","Q");
        var gValue=e.Slider("Gz [kN/m]",-5m,-10m,0m,30,1500);
        var qValue=e.Slider("Qz [kN/m]",-3m,-10m,0m,30,1660);
        var gLoad=e.Component("LoadFrameUniformComponent",1050,1400);e.Wire(align,"Beam",gLoad,"Frame");e.Wire(g,"Load case",gLoad,"Load case");e.Wire(gValue,gLoad,"Load");
        var qLoad=e.Component("LoadFrameUniformComponent",1430,1400);e.Wire(gLoad,"Frame",qLoad,"Frame");e.Wire(q,"Load case",qLoad,"Load case");e.Wire(qValue,qLoad,"Load");
        e.Note("Carichi in sequenza","G -> Q -> Build Model. L'uscita Q contiene entrambi i carichi.\nNon collegare a Build anche la copia intermedia G: duplicheresti il contributo.\nEntrambi sono forze in direzione Z globale, non proiettate.",1030,1610,690,105);
        e.GroupSince(mark,"03 | Trave / due casi / carichi concatenati",Color.SandyBrown);
        mark=e.Mark();
        var uls=e.Component("LoadCombinationComponent",1440,1920);e.Set(uls,"Name","ULS_demo");
        var sls=e.Component("LoadCombinationComponent",1440,2110);e.Set(sls,"Name","SLS_demo");
        var factors=new[]{(g,1.35,uls,1850),(q,1.50,uls,1940),(g,1.00,sls,2050),(q,1.00,sls,2140)};
        foreach(var (lc,factor,combo,y) in factors)
        {
            var f=e.Component("LoadFactorComponent",1040,y);e.Set(f,"Factor",factor);e.Wire(lc,"Load case",f,"Load case");e.Wire(f,"Load factor",combo,"Load factor");
        }
        e.Note("Combinazioni dimostrative","ULS_demo = 1.35 G + 1.50 Q\nSLS_demo = 1.00 G + 1.00 Q\nCoefficienti per mostrare il flusso; adattare al proprio progetto.\nG e' un carico distribuito assegnato: non include peso proprio automatico.",30,1870,730,150);
        e.GroupSince(mark,"04 | Fattori e combinazioni",Color.DarkOrange);
        var units=e.Units(1810,490);
        var modelB=e.Component("BuildModelComponent",1810,1480);e.Wire(qLoad,"Frame",modelB,"Frame element");e.Wire(uls,"Load combination",modelB,"Load combination");e.Wire(sls,"Load combination",modelB,"Load combination");
        e.Wire(units,"Model Units",modelA,"Unit");e.Wire(units,"Model Units",modelB,"Unit");
        var merge=e.Component("MergeModelsComponent",2170,870);e.Set(merge,"Renumber",true);e.Wire(modelA,"Model",merge,"Models");e.Wire(modelB,"Model",merge,"Models");e.FinalModel=merge;
        e.Group("05 | Model B + Model A = un Model",Color.SteelBlue,units,modelB,merge);
        e.Review(merge,2510,470);
        mark=e.Mark();
        e.Note("Prima di avviare il solver","Modifica il percorso .st7 con una destinazione locale scrivibile.\nRun false -> true esporta e calcola casi e combinazioni (licenza API/solver richiesta).\nOverwrite=false protegge un file esistente: cambiare percorso o abilitarlo consapevolmente.\nI filtri Case names / Combination names vuoti includono tutti.\nLo Stream Gate apre i lettori solo con Run=true. Tenere Run=true per leggere i risultati;\ndopo modifiche riportare Run=false, poi true. I risultati restano incorporati nel Model.",1790,2310,880,165);
        var path=e.Note("Percorso .st7 modificabile",@"C:\Temp\Rhino2StrausExamples\02_portale.st7",1790,2530,650,65);
        var run=e.Toggle("Run analisi",false,1900,2660);
        var overwrite=e.Toggle("Overwrite file",false,1900,2730);
        var solve=e.Component("RunModelCasesComponent",2800,2640);e.Wire(merge,"Model",solve,"Model");e.Wire(path,solve,"Model path");e.Wire(run,solve,"Run");e.Wire(overwrite,solve,"Overwrite");
        e.Readout(solve,"Status","Stato analisi",3130,2370,310,100);e.Readout(solve,"Unavailable","Quantita' non disponibili",3130,2500,310,120);
        var gate=e.Add(new Grasshopper.Kernel.Components.GH_StreamGateComponent(),3260,2750);e.Wire(solve,"Model",gate,"Stream");e.Wire(run,gate,"Gate");
        e.GroupSince(mark,"06 | Analisi opzionale / Run inizialmente FALSE",Color.IndianRed);
        mark=e.Mark();
        var cases=e.Component("ModelResultCasesComponent",3760,2310);e.Wire(gate,"Target 1",cases,"Model");e.Readout(cases,"Names","Casi risolti",4110,2250,280,120);
        var actions=e.Component("ModelBeamActionsComponent",3760,2550);e.Wire(gate,"Target 1",actions,"Model");e.Readout(actions,"M2","Momenti M2 [kN m]",4110,2500,280,140);
        var envelope=e.Component("ModelResultEnvelopeComponent",3760,2810);e.Set(envelope,"Column","M2");e.Wire(gate,"Target 1",envelope,"Model");
        var deformed=e.Component("ModelDeformedGeometryComponent",3760,3080);e.Set(deformed,"Scale",50.0);e.Wire(gate,"Target 1",deformed,"Model");
        var diagram=e.Component("ModelBeamDiagramComponent",3760,3260);e.Wire(gate,"Target 1",diagram,"Model");
        var verification=e.Component("ModelVerificationDataComponent",4190,3040);e.Wire(gate,"Target 1",verification,"Model");
        e.Note("Lettura dal Model","Actions: Case=0 legge tutti i casi. Deformata e diagramma: Case=1.\nControlla Numbers / Names del lettore casi prima di cambiare Case.\nCon l'asse 1 verso Z, il carico verticale produce soprattutto M2: seleziona\nM2 negli input Column e Action per vedere il momento nel piano del portale.\nGli avvisi di input vuoto sui lettori sono attesi finche' Run=false.",4090,3290,710,140);
        e.Set(diagram,"Action","M2");e.Set(diagram,"Display axis",1);
        e.GroupSince(mark,"07 | Sollecitazioni / inviluppi / deformata / dati per verificatore",Color.Teal);
        e.Preview(solve,3640,1520,results:true);
        return e;
    }
    public static ExampleDocument Slab(Assembly plugin)
    {
        var e=new ExampleDocument(plugin,"03_piastra_mesh_pressione"){ExpectedCounts=new[]{9,0,4,1,0},ExpectedVolume=2.4};
        e.Note("03 | Piastra / m / kN","Piastra 4 x 3 m suddivisa in 4 Quad4, 9 nodi.\nCalcestruzzo C30/37, spessore 0.20 m, offset +0.10 m.\nAssi locali allineati a (1,1,0); pressione globale (0,0,-5) kN/m2.\nGli 8 nodi al contorno sono incastrati.\nMesh volutamente grossolana per illustrare i collegamenti; infittire per un'analisi.",30,20,900,155);
        int mark=e.Mark();
        var concrete=e.Component("DatabaseConcreteMaterialComponent",340,290);
        var t=e.Slider("Spessore [m]",.20m,.10m,.50m,30,430);
        var thickness=e.Component("AreaThicknessComponent",340,450);e.Set(thickness,"Name","Soletta");e.Wire(t,thickness,"Thickness");
        var property=e.Component("AreaPropertyComponent",670,360);e.Set(property,"Name","Soletta C30/37");e.Wire(concrete,"Concrete Material",property,"Material");e.Wire(thickness,"Area thickness",property,"Section");
        e.GroupSince(mark,"01 | Materiale e spessore",Color.CornflowerBlue);
        mark=e.Mark();
        var mesh=new Mesh();for(int j=0;j<3;j++)for(int i=0;i<3;i++)mesh.Vertices.Add(i*2,j*1.5,0);
        for(int j=0;j<2;j++)for(int i=0;i<2;i++){int k=j*3+i;mesh.Faces.AddFace(k,k+1,k+4,k+3);}mesh.Normals.ComputeNormals();mesh.Compact();
        var input=e.Add(new Param_Mesh{NickName="Mesh 4 Quad4",Hidden=true},30,740);input.SetPersistentData(new GH_Mesh(mesh));
        var plates=e.Component("MeshPlatesComponent",340,740);e.Wire(input,plates,"Mesh");e.Wire(property,"Area Property",plates,"Property");
        var value=e.Slider("Offset [m]",.10m,-.30m,.30m,580,930);
        var offset=e.Component("PlateOffsetComponent",680,740);e.Wire(plates,"Plates",offset,"Plate");e.Wire(value,offset,"Offset");
        var direction=e.Vector("Direzione asse 1",new Vector3d(1,1,0),970,930);
        var align=e.Component("AlignPlateAxisComponent",1050,740);e.Wire(offset,"Plate",align,"Plate");e.Wire(direction,align,"Direction");
        e.Note("Normale e quota fisica","I Quad4 hanno normale +Z. L'offset positivo sposta il volume verso +Z.\nCon t=0.20 e offset=0.10: volume da Z=0 a Z=0.20.\nCambiare gli assi locali non cambia la direzione della pressione globale.",30,900,490,110);
        e.GroupSince(mark,"02 | Mesh / piastre / offset / assi",Color.MediumPurple);
        mark=e.Mark();
        var boundary=mesh.Vertices.ToPoint3dArray().Where(p=>p.X==0||p.X==4||p.Y==0||p.Y==3).ToArray();
        var points=e.Points("8 nodi al contorno",30,1190,boundary);var support=e.FixedSupport(points,340,1190);
        var lc=e.Component("LoadCaseComponent",340,1470);e.Set(lc,"Name","Q_piastra");
        var pressure=e.Vector("Pressione XYZ [kN/m2]",new Vector3d(0,0,-5),680,1490);
        var load=e.Component("GlobalPressureComponent",1050,1390);e.Wire(align,"Plate",load,"Plate");e.Wire(lc,"Load case",load,"Load case");e.Wire(pressure,load,"Pressure");
        e.GroupSince(mark,"03 | Vincoli di bordo e pressione",Color.SandyBrown);
        var units=e.Units(1430,380);
        var model=e.Component("BuildModelComponent",1430,760);e.Wire(units,"Model Units",model,"Unit");e.Wire(support,"Node",model,"Node");e.Wire(load,"Plate",model,"Area element");e.FinalModel=model;
        e.Group("04 | Un solo Model",Color.SteelBlue,units,model);
        e.Review(model,1800,400);e.Preview(model,1800,1220);
        return e;
    }
}
