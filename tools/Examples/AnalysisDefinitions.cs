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
    public static ExampleDocument NativeStandard(Assembly p)=>NativeGallery(p,false);
    public static ExampleDocument NativeBgl(Assembly p)=>NativeGallery(p,true);
    private static ExampleDocument NativeGallery(Assembly p,bool bgl)
    {
        var e=New(p,bgl?"11_sezioni_native_bgl":"10_sezioni_native_standard",bgl?
            "Sezioni BGL: angolare, bulb, canale, I, RHS e T. Le dimensioni sono negli input del componente.\nRichiede API Straus: abilita Genera profili per calcolare rigidezze e contorni. Ogni sezione alimenta una trave e un solo Model.":
            "Sezioni native Straus: cruciforme, canale labbrato, T labbrato, top-hat, trapezio e triangolo pieni/cavi, Z.\nNative Section Options controlla discretizzazione e taglio. API necessaria: Genera profili e' False all'apertura.");
        var names=p.GetTypes().Where(t=>!t.IsAbstract&&t.Name.StartsWith("NativeSection")&&t.Name.EndsWith("Component")&&t.Name!="NativeSectionOptionsComponent"&&t.Name.Contains("Bgl")==bgl).Select(t=>t.Name).Order().ToArray();
        var enabled=e.Toggle("Genera profili / API",false,30,320);var options=e.Component("NativeSectionOptionsComponent",350,320);
        var material=e.Component("DatabaseSteelMaterialComponent",730,320);var model=e.Component("BuildModelComponent",1920,780);
        for(int i=0;i<names.Length;i++)
        {
            int mark=e.Mark();float y=760+i*590;var name=e.Note("Nome sezione",names[i].Replace("Component","").Replace("NativeSection",""),30,y,240,60);var gate=e.Gate(name,enabled,350,y);
            var section=e.Component(names[i],730,y);e.Wire(gate,"Target 1",section,"Name");e.First(options,section,"Options");
            var info=e.Step("SectionInfoComponent",section,"Section",1120,y+230);e.Readout(info,"Area","Area calcolata [m2]",1490,y+260,270,85);
            var property=e.Component("FramePropertyComponent",1110,y);e.Set(property,"Name",names[i]);e.First(section,property,"Section");e.First(material,property,"Material");
            var beam=e.Beam(1310,y+30,property,new Line(0,i,0,2,i,0));e.First(beam,model,"Frame element");
            e.GroupSince(mark,names[i].Replace("Component",""),Color.CornflowerBlue);
        }
        // Wait until the generated model exists before validating or previewing.
        var ready=e.Gate(ExampleDocument.Output(model,"Model"),enabled,2240,780);
        var sink=e.Component("ClearModelResultsComponent",2560,780);e.Wire(ready,"Target 1",sink,"Model");e.Review(sink,2890,350);e.Preview(sink,2890,1450);return e;
    }
    public static ExampleDocument Bxs(Assembly p)
    {
        var e=New(p,"12_sezione_bxs_profilo_bake","Importazione di una sezione .bxs esistente. Imposta il percorso, poi Leggi BXS=True.\nIl componente conserva i dati BXS nel Model per l'export. Section Info espone contorno e proprieta'.\nIl profilo viene estruso lungo una trave; Display Manager espone Breps per bake manuale.");
        var path=e.Note("File BXS da scegliere",@"C:\Temp\sezione.bxs",30,350,620,70);var enabled=e.Toggle("Leggi BXS / API",false,30,510);var gate=e.Gate(path,enabled,730,410);
        var section=e.Component("BxsSectionComponent",1050,410);e.Wire(gate,"Target 1",section,"BXS file");
        var material=e.Component("DatabaseSteelMaterialComponent",1050,650);var property=e.Component("FramePropertyComponent",1410,440);e.First(material,property,"Material");e.First(section,property,"Section");
        var info=e.Step("SectionInfoComponent",section,"Section",1410,750);e.Readout(info,"Definition","Tipo sezione",1800,790,240,90);
        var beam=e.Beam(30,950,property);var model=e.Step("BuildModelComponent",beam,"Frame element",1800,440);
        var ready=e.Gate(ExampleDocument.Output(model,"Model"),enabled,2130,450);var sink=e.Component("ClearModelResultsComponent",2450,450);e.Wire(ready,"Target 1",sink,"Model");
        e.Review(sink,2830,350);e.Preview(sink,2830,1400);return e;
    }
    public static ExampleDocument Files(Assembly p)
    {
        var e=New(p,"16_export_import_model_esistente","1. Il modello dimostrativo puo' essere esportato con Run. 2. Read Straus Model legge quel file o un altro .st7.\n3. Controlla Complete, Report e Identity map: il Model contiene geometrie, proprieta', carichi e attributi supportati.\nIl file sorgente non viene salvato. L'import normalizza in metri e kN; Allow partial=False blocca il Model in presenza di omissioni.\nLa lettura geometrica e il bake diretto dal file sono alternative. Ogni azione ha un interruttore indipendente.");
        var f=e.CantileverModel();var path=e.Note("Percorso di lavoro / cambia per importare un file esistente",@"C:\Temp\Rhino2StrausExamples\16_model.st7",1640,330,830,75);
        var export=e.Step("StrausR3ExportComponent",f.Model,"Model",2110,790);e.Wire(path,export,"Output Path");e.Triggers(export,1640,1110);e.Readout(export,"Status","Stato export",2460,770,400,100);
        var import=e.Component("ReadStrausModelComponent",3090,760);e.Wire(path,import,"Model path");e.Triggers(import,2690,1020);
        e.Readout(import,"Report","Rapporto import / controllare omissioni",3460,1020,680,300);e.Readout(import,"Identity map","Numeri e label originali",3460,1370,680,240);
        var d=e.Step("DecomposeModelComponent",import,"Model",3500,680);var rebuild=e.Component("BuildModelComponent",3940,680);
        foreach(var pair in new[]{("Node elements","Node"),("Frame elements","Frame element"),("Area elements","Area element"),("Link elements","Link element"),("Solids","Solids"),("Combinations","Load combination"),("Units","Unit"),("Self weight","Self weight"),("Preserve nodes","Preserve nodes"),("Load cases","Load cases")})e.Wire(d,pair.Item1,rebuild,pair.Item2);
        e.Note("Modifica e ricostruisci","Inserisci qui i componenti di modifica su travi, piastre o nodi.\nSostituisci il collegamento diretto: collegare insieme originale e copia modificata puo' duplicare attributi.\nPreserve nodes=True conserva nodi coincidenti con ID diversi; le unita' arrivano dal file importato.",4380,20,900,160);
        var second=e.Step("StrausR3ExportComponent",rebuild,"Model",4460,800);e.Set(second,"Output Path",@"C:\Temp\Rhino2StrausExamples\16_modificato.st7");e.Triggers(second,4450,1100);
        e.Preview(import,4760,1500);e.Review(import,4760,350);
        var geometryEnable=e.Toggle("Leggi solo geometria / API",false,1650,1840);var gate=e.Gate(path,geometryEnable,2110,1770);
        var geometry=e.Component("ReadStrausGeometryComponent",2530,1770);e.Wire(gate,"Target 1",geometry,"Model path");e.Readout(geometry,"Node IDs","Label file originale",2910,1830,450,110);
        var bake=e.Component("BakeStrausGeometryComponent",2540,2140);e.Wire(path,bake,"Model path");e.Triggers(bake,2140,2180);e.Readout(bake,"Object IDs","ID oggetti Rhino creati",2910,2170,450,100);
        e.FinalModel=null;return e;
    }
    public static ExampleDocument Modal(Assembly p)
    {
        var e=New(p,"17_analisi_modale","Mensola con massa strutturale: Run Model Natural Frequencies esporta, esegue e incorpora i risultati.\nImposta percorso e numero modi. Le ampiezze modali dipendono dalla normalizzazione Straus: non sono spostamenti sotto Q.\nIl Model contiene frequenze, masse modali, casi e forme deformate; tutti i comandi sono inizialmente False.");
        var f=e.CantileverModel();var solver=e.Step("RunModelModalComponent",f.Model,"Model",1820,760);e.Set(solver,"Model path",@"C:\Temp\Rhino2StrausExamples\17_modal.st7");e.Set(solver,"Modes",3);e.Triggers(solver,1460,1090);
        var gate=e.ResultGate(solver,2230,760);var meta=e.Component("ModelModalMetadataComponent",2610,700);e.Wire(gate,"Target 1",meta,"Model");e.Readout(meta,"Frequency Hz","Frequenze [Hz]",2950,730,390,120);
        var deformed=e.Component("ModelDeformedGeometryComponent",2610,1050);e.Wire(gate,"Target 1",deformed,"Model");e.Set(deformed,"Scale",1.0);
        e.Readout(solver,"Status","Stato",2220,1130,390,100);e.ExpectedCounts=new[]{2,1,0,1,0};e.Finish(solver,3450,350);return e;
    }
    public static ExampleDocument Buckling(Assembly p)
    {
        var e=New(p,"18_analisi_buckling","Mensola compressa. Prima esegui statica; poi abilita il solver buckling che riceve Initial LSA dalla statica.\nVariable result case=1, Fixed result case=0. Il fattore critico moltiplica il carico di riferimento.\nUsa lo stesso modello di input nei due solver. Il cambio dei dati richiede una nuova statica. Le forme sono autovettori normalizzati.");
        var f=e.CantileverModel();var load=e.Document.Objects.OfType<GH_Component>().Single(c=>c.GetType().Name=="LoadNodalComponent");e.Set(load,"Fz",0.0);e.Set(load,"Fx",-10.0);
        var stat=e.StaticSolver(f.Model,1860,760,"18_static");var ready=e.ResultGate(stat,2290,770);
        var solver=e.Component("RunModelBucklingComponent",2700,760);e.Wire(ready,"Target 1",solver,"Model");e.Wire(stat,"Result path",solver,"Initial LSA");e.Set(solver,"Model path",@"C:\Temp\Rhino2StrausExamples\18_buckling.st7");e.Set(solver,"Modes",3);e.Triggers(solver,2340,1200);
        var gate=e.ResultGate(solver,3100,750);var meta=e.Component("ModelModalMetadataComponent",3490,730);e.Wire(gate,"Target 1",meta,"Model");e.Readout(meta,"Buckling factor","Fattori critici",3870,750,370,130);
        var def=e.Component("ModelDeformedGeometryComponent",3500,1030);e.Wire(gate,"Target 1",def,"Model");
        e.ExpectedCounts=new[]{2,1,0,1,0};e.Finish(f.Model,4360,350);return e;
    }
    public static ExampleDocument Nonlinear(Assembly p)
    {
        var e=New(p,"19_analisi_non_lineare","Analisi statica con non linearita' geometrica della mensola; Q viene applicato in dieci incrementi.\nNonlinear geometry=True, materiale elastico. Non e' una legge plastica del materiale.\nModel Result Cases espone gli incrementi disponibili; leggere valori e diagrammi per il caso desiderato.");
        var f=e.CantileverModel();var solver=e.Step("RunModelNonlinearComponent",f.Model,"Model",1850,760);e.Set(solver,"Model path",@"C:\Temp\Rhino2StrausExamples\19_nonlinear.st7");e.Set(solver,"Case name","Q");e.Triggers(solver,1450,1120);
        var gate=e.ResultGate(solver,2230,760);var cases=e.Component("ModelResultCasesComponent",2610,720);e.Wire(gate,"Target 1",cases,"Model");e.Readout(cases,"Names","Incrementi risolti",2990,710,490,180);
        var actions=e.Component("ModelBeamActionsComponent",2610,1060);e.Wire(gate,"Target 1",actions,"Model");e.Readout(actions,"M2","Momenti M2",3000,1060,490,180);
        e.ExpectedCounts=new[]{2,1,0,1,0};e.Finish(solver,3690,350);return e;
    }
    private static void TypedReaders(ExampleDocument e,GH_Component gate,IEnumerable<string> names,float x,float y)
    {
        int i=0;foreach(string name in names)
        {
            float row=y+(i/3)*580,col=x+(i%3)*840;var reader=e.Component(name,col,row);e.Wire(gate,"Target 1",reader,"Model");
            e.Readout(reader,"Columns","Colonne / "+reader.Name,col+350,row-100,390,120);
            e.Readout(reader,"Values","Valori / "+reader.Name,col+350,row+70,390,180);
            if(i==0)
            {
                var record=e.Component("DecomposeResultRecordComponent",col,row+275);e.Wire(reader,"Records",record,"Record");e.Readout(record,"Entity ID","ID nel Model",col+350,row+300,390,90);
            }
            i++;
        }
    }
    public static ExampleDocument BeamResults(Assembly p)
    {
        var e=New(p,"20_risultati_nodi_travi_link","Tutti i lettori Model per nodi, travi e link. Prima avvia la statica.\nCase=0 legge tutti i casi; IDs vuoto legge tutti gli elementi. Records conserva caso, ID, numero nativo, assi, colonne e campionamento.\nValues usa rami per caso/elemento/superficie. Controlla Columns e Unavailable: una quantita' non disponibile non equivale a zero.");
        var f=e.CantileverModel();var link=e.Component("LinkElementComponent",820,1640);e.Set(link,"Start Point",new Point3d(4,0,0));e.Set(link,"End Point",new Point3d(4,0,1));e.First(link,f.Model,"Link element");
        var solver=e.StaticSolver(f.Model,1830,770);var gate=e.ResultGate(solver,2230,770);
        TypedReaders(e,gate,new[]{"ModelNodeDisplacementComponent","ModelNodeReactionComponent","ModelLinkNodeDisplacementComponent","ModelLinkNodeReactionComponent","ModelBeamDisplacementComponent","ModelBeamForceComponent","ModelBeamNodeReactionComponent","ModelBeamStrainComponent","ModelBeamStressComponent","ModelBeamTotalStrainComponent"},2650,400);
        var sections=e.Component("ModelNativeSectionsComponent",2700,2940);e.Wire(gate,"Target 1",sections,"Model");var decompose=e.Step("DecomposeNativeSectionComponent",sections,"Section",3130,2940);e.Readout(decompose,"Area","Area effettivamente usata dal solver",3520,2980,450,120);
        var verify=e.Component("ModelVerificationDataComponent",4120,2940);e.Wire(gate,"Target 1",verify,"Model");e.Readout(verify,"Unavailable","Dati mancanti per verifica",4490,2980,480,140);
        e.ExpectedCounts=new[]{3,1,0,1,0};e.ExpectedLinks=1;e.Finish(solver,5250,350);return e;
    }
    public static ExampleDocument PlateSolidResults(Assembly p)
    {
        var e=New(p,"21_risultati_piastre_solidi","Model misto: una piastra Quad4 e un Hexa8 con appoggi e pressione.\nLettura di forze, momenti, curvature, tensioni, deformazioni, reazioni e spostamenti nodali per elemento.\nLe famiglie di risultati seguono i campionamenti e gli assi dichiarati nei Records. Controlla il lato/superficie delle piastre.");
        var prop=e.PlateProperty(30,320);var plate=e.Component("AreaElementComponent",350,720);e.Wire(e.MeshInput("Piastra",PanelMesh(0),30,720),plate,"Area");e.First(prop,plate,"Area Property");var lc=e.Case("P",30,1000);
        var pressure=e.Step("GlobalPressureComponent",plate,"Plate",730,720);e.First(lc,pressure,"Load case");e.Set(pressure,"Pressure",new Vector3d(0,0,-5));
        var pts=SolidPoints()[2];var nodes=e.Component("NodeElementComponent",350,1400);e.Wire(e.Points("Hexa8",30,1400,pts),nodes,"Node point");
        var solid=e.Step("SolidElementComponent",nodes,"Nodes",720,1400);var mat=e.Component("DatabaseConcreteMaterialComponent",350,1200);e.First(mat,solid,"Material");
        var sp=e.Step("SolidPressureComponent",solid,"Solid",1100,1400);e.First(lc,sp,"Load case");e.Set(sp,"Pressure",5.0);
        var model=e.Component("BuildModelComponent",1460,750);e.First(pressure,model,"Area element");e.First(sp,model,"Solids");
        var fix=e.FixedSupport(e.Points("Appoggi",30,1900,PanelMesh(0).Vertices.ToPoint3dArray().Concat(pts.Where(n=>n.Z==0)).ToArray()),350,1900);e.First(fix,model,"Node");
        var solver=e.StaticSolver(model,1900,760);var gate=e.ResultGate(solver,2310,760);
        TypedReaders(e,gate,new[]{"ModelPlateCurvatureComponent","ModelPlateForceComponent","ModelPlateMomentComponent","ModelPlateNodeDisplacementComponent","ModelPlateNodeReactionComponent","ModelPlateStrainComponent","ModelPlateStressComponent","ModelPlateTotalStrainComponent","ModelSolidNodeDisplacementComponent","ModelSolidNodeReactionComponent","ModelSolidStrainComponent","ModelSolidStressComponent","ModelSolidTotalStrainComponent"},2700,400);
        e.ExpectedCounts=new[]{12,0,1,1,0};e.ExpectedSolids=1;e.Finish(solver,5350,350);return e;
    }
    public static ExampleDocument FileResults(Assembly p)
    {
        var e=New(p,"22_file_esistenti_solver_risultati","Lettori diretti di file Straus: .st7 e risultati gia' calcolati (.LSA / .NFA / .LBA / .NLA).\nScegli file corrispondenti e numeri nativi degli elementi, non le label. Leggi=True apre i lettori; non avvia un solver.\nLa zona inferiore mostra i solver su file esistente, con pulsanti separati. Non sovrascrivere un file di produzione per provare l'esempio.\nLa geometria deformata usa punti e spostamenti con la stessa sequenza di numeri nodo.");
        var path=e.Note("Modello esistente",@"C:\Temp\model.st7",30,380,640,70);var result=e.Note("Risultato statico",@"C:\Temp\model.LSA",30,520,640,70);
        var enabled=e.Toggle("Leggi risultati esistenti / API",false,30,700);var pg=e.Gate(path,enabled,810,390);var rg=e.Gate(result,enabled,810,590);
        var names=new[]{"StrausResultCasesComponent","NodeDisplacementsComponent","NodeReactionsComponent","BeamForcesComponent","BeamStressComponent","BeamDisplacementResultsComponent","PlateForceResultsComponent","PlateMomentResultsComponent","PlateStressResultsComponent","SolidStrainResultsComponent","SolidStressResultsComponent"};
        GH_Component displacement=null;
        for(int i=0;i<names.Length;i++)
        {
            float x=1220+(i%3)*880,y=430+(i/3)*620;var reader=e.Component(names[i],x,y);e.Wire(pg,"Target 1",reader,"Model path");e.Wire(rg,"Target 1",reader,"Result path");
            foreach(var input in reader.Params.Input.Where(q=>q.Name.EndsWith("number")||q.Name=="Node numbers"))e.Set(reader,input.Name,1);
            e.Readout(reader,reader.Params.Output[0].Name,"Valori / "+reader.Name,x+370,y,400,180);
            if(names[i]=="NodeDisplacementsComponent")displacement=reader;
            if(names[i]=="BeamForcesComponent")
            {var env=e.Component("ResultEnvelopeComponent",x,y+290);e.Wire(reader,"M2",env,"Cases");e.Readout(env,"Maximum","Inviluppo dei rami M2",x+370,y+290,400,120);}
        }
        var geometry=e.Component("ReadStrausGeometryComponent",1220,3020);e.Wire(pg,"Target 1",geometry,"Model path");e.Wire(geometry,"Node numbers",displacement,"Node numbers");
        var deformed=e.Component("DeformedNodesComponent",2110,3020);e.Wire(geometry,"Nodes",deformed,"Points");e.Wire(displacement,"Translation",deformed,"Displacements");
        e.Note("Campionamento e identificazione","I file statici devono contenere gli elementi richiesti. Sostituire 1 negli input Beam/Plate/Solid number.\nInviluppo aggrega i rami forniti: collegare piu' casi per confrontare carichi diversi.\nSe il file non contiene piastre o solidi, usare soltanto i lettori della famiglia presente.",30,1230,1000,160);
        var modal=e.Note("Risultato modale",@"C:\Temp\model.NFA",30,3570,620,70);var buck=e.Note("Risultato buckling",@"C:\Temp\model.LBA",30,3830,620,70);
        var mg=e.Gate(modal,enabled,810,3570);var bg=e.Gate(buck,enabled,810,3830);
        var mr=e.Component("ModalResultsComponent",1230,3570);e.Wire(pg,"Target 1",mr,"Model path");e.Wire(mg,"Target 1",mr,"Result path");e.Readout(mr,"Frequency Hz","Frequenze",1620,3580,370,100);
        var br=e.Component("BucklingFactorsComponent",1230,3830);e.Wire(pg,"Target 1",br,"Model path");e.Wire(bg,"Target 1",br,"Result path");e.Readout(br,"Factors","Fattori",1620,3840,370,100);
        int solverIndex=0;foreach(string type in new[]{"RunLinearStaticComponent","RunNaturalFrequencyComponent","RunLinearBucklingComponent","RunNonlinearStaticComponent"})
        {
            float x=800+solverIndex*940;var solver=e.Component(type,x,4500);e.Wire(path,solver,"Model path");e.Triggers(solver,x-330,4810);
            if(type=="RunLinearBucklingComponent")e.Wire(result,solver,"Initial results");
            e.Readout(solver,"Status","Stato / "+solver.Name,x+350,4520,450,130);solverIndex++;
        }
        return e;
    }
    public static ExampleDocument Reread(Assembly p)
    {
        var e=New(p,"23_rilettura_risultati_nel_model","Read Results Into Model ricollega un file risultati al suo Model di input.\nLa statica di questo esempio crea .st7, .LSA e manifest; Read li rilegge verificando identita' e provenienza.\nPer file esterni creati senza Rhino2Straus usare l'esempio 22; il nuovo import strutturale richiede riesportazione e ricalcolo per ottenere questa provenienza.\nIl risultato rimane nel Model per il futuro verificatore; una modifica strutturale invalida i vecchi risultati.");
        var f=e.CantileverModel();var solver=e.StaticSolver(f.Model,1820,760);var read=e.Component("ReadResultsIntoModelComponent",2660,760);
        e.First(f.Model,read,"Model");e.Set(read,"Model path",@"C:\Temp\Rhino2StrausExamples\"+e.FileName+".st7");e.Wire(solver,"Result path",read,"Result path");e.Triggers(read,2280,1190);
        var toggle=(Grasshopper.Kernel.Special.GH_BooleanToggle)ExampleDocument.Input(read,"Read").Sources.Single();var gate=e.Gate(ExampleDocument.Output(read,"Model"),toggle,3070,760);
        var cases=e.Component("ModelResultCasesComponent",3490,680);e.Wire(gate,"Target 1",cases,"Model");e.Readout(cases,"Provenance verified","Provenienza verificata",3890,680,440,100);
        var verify=e.Component("ModelVerificationDataComponent",3490,1010);e.Wire(gate,"Target 1",verify,"Model");e.Readout(verify,"Unavailable","Dati da integrare",3890,1010,440,180);
        e.ExpectedCounts=new[]{2,1,0,1,0};e.Finish(f.Model,4650,350);return e;
    }
}
