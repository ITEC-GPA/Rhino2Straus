using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rhino.Geometry;
using Rhino2Straus.Api;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Loads;
using Rhino2Straus.Core.Models;
using Rhino2Straus.Core.Settings;
using Rhino2Straus.Import;
using St7API;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
#if NATIVE_TEST_HOST
        RhinoInside.Resolver.Initialize(Environment.GetEnvironmentVariable("RHINO_SYSTEM_DIRECTORY")
            ?? @"C:\Program Files\Rhino 8\System");
        return RunNative(args.Concat(new[] { "--native" }).ToArray());
#else
        if (args.Contains("--native"))
        {
            Console.Error.WriteLine("Use: dotnet run --project tools/NativeTests -c Release -- --solve");
            return 2;
        }
        return Run(args);
#endif
    }

#if NATIVE_TEST_HOST
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static int RunNative(string[] args)
    {
        using var rhino = new Rhino.Runtime.InProcess.RhinoCore(new[] { "/nosplash" }, Rhino.Runtime.InProcess.WindowStyle.NoWindow);
        return Run(args);
    }
#endif

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static int Run(string[] args)
    {
int failures = 0, passed = 0;
void Test(string name, Action test)
{
    try { test(); Console.WriteLine("PASS " + name); passed++; }
    catch (Exception e) { failures++; Console.WriteLine("FAIL " + name + ": " + e); }
}
void Assert(bool value, string message = "Assertion failed") { if (!value) throw new Exception(message); }
void Near(double actual, double expected, double tolerance = 1e-10) => Assert(Math.Abs(actual - expected) <= tolerance, $"Expected {expected}, got {actual}");
void Throws(Action action) { try { action(); } catch { return; } throw new Exception("Expected an exception"); }
void Check(int error) => StrausRuntime.Check(error, "Integration test");

var loadCase = new LoadCaseModel("Tip load");
var material = MaterialModel.S355;
var section = new FrameSectionModel { Name = "Generic", SectionGeometryType = FrameSectionModel.SectionGeometryTypes.Generic, SectionArea = .01, I11 = .0001, I22 = .0001, J = .0002, ShearA1 = .01, ShearA2 = .01, Breps = new() };
var property = new FramePropertyModel("Test beam", material, section);
FrameElementModel Beam(NodeElementModel start, NodeElementModel end) => new() { NodeStart = start, NodeEnd = end, FrameProperty = property, FrameSection = section, Material = material, Offset = Vector3d.Zero };
ModelModel Cantilever()
{
    var start = new NodeElementModel(0, 0, 0) { Id = 10, Support = new NodeSupportModel(true, true, true, true, true, true) };
    var end = new NodeElementModel(2, 0, 0) { Id = 50 };
    end.NodalLoadList.Add(new NodalLoadModel(loadCase, 0, 0, -.4, 0, 0, 0));
    end.NodalLoadList.Add(new NodalLoadModel(loadCase, 0, 0, -.6, 0, 0, 0));
    var model = new ModelModel(); model.BuildModel(new() { start, end }, new() { Beam(start, end) }); return model;
}

Test("All length and temperature units map to independent API indices", () => {
    foreach (var l in Enum.GetValues<ModelUnitsModel.LengthUnitTypes>()) foreach (var t in Enum.GetValues<ModelUnitsModel.TemperatureUnitTypes>()) {
        var u = new ModelUnitsModel { LengthUnit = l, TemperatureUnit = t, ForceUnit = ModelUnitsModel.ForceUnitTypes.KN };
        var mapped = StrausUnits.ToApi(u); Assert(mapped[St7.ipFORCEU] == St7.fuKILONEWTON);
        Assert(mapped[St7.ipTEMPERU] == (t == ModelUnitsModel.TemperatureUnitTypes.C ? St7.tuCELSIUS : St7.tuFAHRENHEIT));
        Assert(mapped[St7.ipLENGTHU] == (l == ModelUnitsModel.LengthUnitTypes.M ? St7.luMETRE : l == ModelUnitsModel.LengthUnitTypes.CM ? St7.luCENTIMETRE : St7.luMILLIMETRE));
    }
});
Test("Unsupported unit fails explicitly", () => Throws(() => StrausUnits.ToApi(new ModelUnitsModel { HeatUnit = ModelUnitsModel.HeatUnitTypes.KCAL })));
Test("Gravity copy preserves all settings", () => { var w = SelfWeightModel.DeadLoad; w.GravityValue = -9.81; var c = new SelfWeightModel(w); Near(c.GravityValue, -9.81); Assert(c.StructuralMassAcceleration && c.GlobalInertiaLoad == w.GlobalInertiaLoad); });
Test("Node copy isolates support and load collections", () => { var n = new NodeElementModel(1,2,3); n.NodalLoadList.Add(new NodalLoadModel(loadCase,1,2,3,4,5,6)); var c = new NodeElementModel(n); c.Support.Dx = true; c.NodalLoadList[0].FX = 100; c.NodalLoadList.Clear(); Assert(!n.Support.Dx && n.NodalLoadList.Count == 1); Near(n.NodalLoadList[0].FX,1); });
Test("Beam copy preserves offset and isolates releases", () => { var b = Beam(new NodeElementModel(),new NodeElementModel(1,0,0)); b.Offset = new Vector3d(1,2,3); var c = new FrameElementModel(b); Near(c.Offset.Y,2); c.StartReleases[0]=true; Assert(!b.StartReleases[0]); c.FrameLoadList.Add(new FramePreloadModel()); Assert(b.FrameLoadList.Count==0); });
Test("Plate copy preserves bending thickness and isolates connectivity", () => { var t = new AreaThicknessModel("Plate",.2,.3,.1); var c = new AreaThicknessModel(t); Near(c.ThicknessBending,.3); var p = new AreaElementModel { NodeList = new() { new NodeElementModel() }, Groups = new(), AreaLoadList = new() }; var p2 = new AreaElementModel(p); p2.NodeList.Clear(); Assert(p.NodeList.Count==1); });
Test("Material copy preserves thermal properties and density convention", () => { var m = MaterialModel.S355; m.Conductivity=13;m.SpecificHeat=25;m.ViscousDamping=.1;var c=new MaterialModel(m);Near(c.Conductivity,13);Near(c.SpecificHeat,25);Near(c.Density,7.849); });
Test("Hierarchical groups preserve parent order and copy", () => { var g = ElementGroupModel.ConvertStringToGroup("Building::Floor::Beams"); Assert(g.Name=="Beams" && g.GroupFullName=="Building::Floor::Beams"); Assert(new ElementGroupModel(g).GroupFullName==g.GroupFullName); });
Test("Build retains canonical connectivity, loads and IDs", () => { var m=Cantilever();Assert(m.NodeElements.Count==2 && m.FrameElements.Count==1);Assert(m.NodeElements.ContainsKey(10)&&m.NodeElements.ContainsKey(50));var b=m.FrameElements.Values.Single();Assert(ReferenceEquals(b.NodeEnd,m.NodeElements[50]));Assert(m.NodeElements[50].NodalLoadList.Count==2);Assert(m.FrameProperties.Count==1 && m.LoadCases.Count==1); });
Test("Build leaves inputs untouched and keeps offsets", () => {var a=new NodeElementModel();var b=new NodeElementModel(1,0,0);var beam=Beam(a,b);beam.Offset=new Vector3d(.1,.2,0);var m=new ModelModel();m.BuildModel(inputFrames:new(){beam});Assert(a.Id==-1&&b.Id==-1&&beam.Id==-1&&material.Id==-1);Near(m.FrameElements.Values.Single().Offset.Y,.2);});
Test("Spatial merge accumulates nodal loads and restraints", () => {var a=new NodeElementModel(0,0,0);a.Support.Dx=true;var b=new NodeElementModel(.0005,0,0);b.Support.Dz=true;b.NodalLoadList.Add(new NodalLoadModel(loadCase,0,0,5,0,0,0));var m=new ModelModel();m.BuildModel(new(){a,b});Assert(m.NodeElements.Count==1);var n=m.NodeElements.Values.Single();Assert(n.Support.Dx&&n.Support.Dz&&n.NodalLoadList.Count==1);});
Test("Conflicting node IDs fail without corrupting existing model", () => {var m=Cantilever();Throws(()=>m.BuildModel(new(){new NodeElementModel(0,0,0){Id=1},new NodeElementModel(10,0,0){Id=1}}));Assert(m.NodeElements.Count==2);});
Test("Duplicate beam IDs rejected", () => {var a=new NodeElementModel();var b=new NodeElementModel(1,0,0);var x=Beam(a,b);var y=Beam(a,new NodeElementModel(3,0,0));x.Id=y.Id=20;Throws(()=>new ModelModel().BuildModel(inputFrames:new(){x,y}));});
Test("Build can replace a previous model",()=>{var m=Cantilever();m.BuildModel(new(){new NodeElementModel(4,5,6)});Assert(m.NodeElements.Count==1&&m.FrameElements.Count==0&&m.LoadCases.Count==0);});
Test("Model copy retains units and full property graph",()=>{var m=Cantilever();m.ModelUnits.LengthUnit=ModelUnitsModel.LengthUnitTypes.CM;var c=new ModelModel(m);Assert(c.ModelUnits.LengthUnit==ModelUnitsModel.LengthUnitTypes.CM && c.FrameProperties.Count==1);c.NodeElements[10].Support.Dx=false;Assert(m.NodeElements[10].Support.Dx);});
Test("Combination IDs are unique and referenced cases are collected",()=>{var combos=new List<LoadCombinationModel>{new("A",new(){new(loadCase,1.2)},LoadCombinationModel.LoadCombinationTypes.Linear),new("B",new(){new(loadCase,1.5)},LoadCombinationModel.LoadCombinationTypes.Linear)};var m=new ModelModel();m.BuildModel(new(){new NodeElementModel()},inputcombos:combos);Assert(m.LoadCombinations.Values.Select(x=>x.Id).Distinct().Count()==2&&m.LoadCases.Count==1);});
Test("Valid cantilever passes preflight",()=>{var v=ModelValidator.Validate(Cantilever());Assert(v.IsValid,string.Join("; ",v.Errors));});
Test("Zero-length beam rejected before API calls",()=>{var m=Cantilever();m.FrameElements.Values.Single().NodeEnd=m.NodeElements[10];Assert(!ModelValidator.Validate(m).IsValid);});
Test("Global distributed moment rejected instead of becoming a force",()=>{var m=Cantilever();m.FrameElements.Values.Single().FrameLoadList.Add(new FrameLoadStrausModel(loadCase,FrameLoadStrausModel.FrameLoadTypes.Moment,FrameLoadStrausModel.LoadDirections.X,false,0,1,1,1,FrameLoadStrausModel.LoadSchemas.Uniform,CoordinateSystemModel.Global));Assert(!ModelValidator.Validate(m).IsValid);});
Test("Negative mass and invalid modulus rejected",()=>{var m=Cantilever();m.NodeElements[10].TranslationMass=new Vector3d(-1,0,0);m.Materials.Values.Single().ModulusElasticity=0;Assert(ModelValidator.Validate(m).Errors.Count>=2);});
Test("Result buffers split by station without transposition",()=>{var t=StrausFileService.Split(new[]{1d,2,3,4,5,6},2,3);Near(t[1][0],4);Near(t[0][2],3);Throws(()=>StrausFileService.Split(new double[3],2,2));});


Test("Merge independent parts renumbers collisions and welds shared nodes",()=>{
    var a=new ModelModel();a.BuildModel(inputFrames:new(){Beam(new(0,0,0),new(1,0,0))});
    var b=new ModelModel();b.BuildModel(inputFrames:new(){Beam(new(1,0,0),new(2,0,0))});
    var m=ModelComposer.Merge(new[]{a,b},true);Assert(m.FrameElements.Count==2&&m.NodeElements.Count==3);Assert(a.NodeElements.Count==2&&b.NodeElements.Count==2);
});
Test("Merge incompatible units fails before assembly",()=>{var a=Cantilever();var b=Cantilever();b.ModelUnits.LengthUnit=ModelUnitsModel.LengthUnitTypes.MM;Throws(()=>ModelComposer.Merge(new[]{a,b},true));});
Test("Matching beam IDs union attributes without duplicate geometry",()=>{
    var a=Beam(new(0,0,0){Id=1},new(1,0,0){Id=2});a.Id=10;var b=new FrameElementModel(a);a.FrameLoadList.Add(new FramePointLoadModel{LoadCase=loadCase,Value=new(0,0,-2),Position=.2});b.FrameLoadList.Add(new FramePointLoadModel{LoadCase=loadCase,Value=new(0,0,-3),Position=.7});
    var m=new ModelModel();m.BuildModel(inputFrames:new(){a,b});Assert(m.FrameElements.Count==1&&m.FrameElements[10].FrameLoadList.Count==2);
});
Test("Imposed displacements survive copy and reject incompatible node merge",()=>{
    var a=new NodeElementModel(0,0,0);a.Support.Dz=true;a.PrescribedDisplacement[2]=.01;
    var m=new ModelModel();m.BuildModel(new(){a});Near(new ModelModel(m).NodeElements.Values.Single().PrescribedDisplacement[2],.01);
    var b=new NodeElementModel(a);b.PrescribedDisplacement[2]=.02;Throws(()=>new ModelModel().BuildModel(new(){a,b}));
});
Test("Beam loads are independent after model copy",()=>{
    var m=Cantilever();m.FrameElements.Values.Single().FrameLoadList.Add(new FramePointLoadModel{LoadCase=loadCase,Value=new(1,2,3),Position=.5});
    var c=new ModelModel(m);((FramePointLoadModel)c.FrameElements.Values.Single().FrameLoadList[0]).Value=new(9,9,9);
    Near(((FramePointLoadModel)m.FrameElements.Values.Single().FrameLoadList[0]).Value.X,1);
});
Test("Generic sections with different stiffness are not conflated",()=>{var a=new FrameSectionModel(section);var b=new FrameSectionModel(section);b.I11*=2;Assert(!a.Equals(b));});
Test("Beam behaviour survives property copy",()=>{var p=new FramePropertyModel("Cable",material,section){Behaviour=FramePropertyModel.Behaviours.Cable};Assert(new FramePropertyModel(p).Behaviour==FramePropertyModel.Behaviours.Cable);});
Test("Axial beam offset is rejected instead of silently dropped",()=>{var m=Cantilever();m.FrameElements.Values.Single().Offset=new(0,0,1);Assert(!ModelValidator.Validate(m).IsValid);});
SolidElementModel Tet()=>new(){Material=material,Nodes=new(){new(0,0,0),new(1,0,0),new(0,1,0),new(0,0,1)}};
Test("Tet volume and connectivity enter the same model graph",()=>{var t=Tet();t.Pressures.Add(new(loadCase,1,10));Near(t.SignedVolume(),1d/6);var m=new ModelModel();m.BuildModel(inputSolids:new(){t});Assert(m.SolidElements.Count==1&&m.NodeElements.Count==4&&m.LoadCases.Count==1);Assert(m.SolidElements[1].Nodes.All(n=>ReferenceEquals(n,m.NodeElements[n.Id])));Assert(ModelValidator.Validate(m).IsValid);});
Test("Inverted solids are rejected",()=>{var t=Tet();(t.Nodes[1],t.Nodes[2])=(t.Nodes[2],t.Nodes[1]);var m=new ModelModel();m.BuildModel(inputSolids:new(){t});Assert(!ModelValidator.Validate(m).IsValid);});
Test("Hexa8 volume respects native node ordering",()=>{var t=new SolidElementModel{Material=material,Nodes=new(){new(0,0,0),new(2,0,0),new(2,3,0),new(0,3,0),new(0,0,4),new(2,0,4),new(2,3,4),new(0,3,4)}};Near(t.SignedVolume(),24);});
Test("Solid merge and copy retain material and loads",()=>{var t=Tet();t.Pressures.Add(new(loadCase,3,5));var m=new ModelModel();m.BuildModel(inputSolids:new(){t});var c=ModelComposer.Merge(new[]{m},true);Assert(c.SolidElements.Count==1&&c.SolidElements[1].Pressures.Count==1);c.SolidElements[1].Nodes[0].X=20;Near(m.SolidElements[1].Nodes[0].X,0);});
Test("Elastic connection links pass preflight",()=>{var m=new ModelModel();m.BuildModel(inputLinks:new(){new LinkElementModel{NodeStart=new(0,0,0),NodeEnd=new(1,0,0),LinkProperty=new(LinkPropertyModel.LinkPropertyTypes.GEN,100,200,300,10,20,30)}});Assert(ModelValidator.Validate(m).IsValid);});
Test("Steel yield and ultimate strengths keep their intended meaning",()=>{Assert(MaterialModel.S355.TensileYeld==355&&MaterialModel.S355.TensileStrength==510);});
Test("Load case copy retains nonstructural mass flag and label",()=>{var c=new LoadCaseModel("T"){Id=42,GravityNoStructuralMass=true};var x=new LoadCaseModel(c);Assert(x.Id==42&&x.GravityNoStructuralMass);});
Test("Clear resets automatic property numbering",()=>{var c=new Rhino2Straus.Core.Collections.UniqueIdCollection<MaterialModel>();c.Add(new MaterialModel(material){Id=100});c.Clear();c.Add(new MaterialModel(material){Id=-1});Assert(c.ContainsKey(1));});


AnalysisResultsModel SampleResults(ModelModel m, bool verified=true, bool issue=false)
{
    return new(ModelFingerprint.Compute(m),"model.st7","model.LSA","LSA",verified,
        new[]{new ResultCaseInfo(1,"Tip load",false)},
        new[]{new ElementResult(1,ResultEntity.Node,50,2,ResultQuantity.Displacement,0,"Global XYZ","Node",new[]{"UX","UY","UZ","RX","RY","RZ"},new[]{0d},new[]{new[]{0d,0,-.01,0,0,0}})},
        issue?new[]{new ResultIssue(1,ResultEntity.Beam,1,ResultQuantity.Stress,0,"Unavailable")}:Array.Empty<ResultIssue>());
}
Test("Analysis identity survives a deep Model copy",()=>{var m=Cantilever();Assert(ModelFingerprint.Compute(m)==ModelFingerprint.Compute(new ModelModel(m)));});
Test("Analysis identity includes loads, stiffness, releases, offsets and geometry",()=>{
    foreach(Action<ModelModel> change in new Action<ModelModel>[] {m=>m.NodeElements[50].X+=1,m=>m.NodeElements[50].NodalLoadList[0].FZ-=1,
        m=>m.Materials.Values.Single().ModulusElasticity*=.9,m=>m.FrameElements.Values.Single().StartReleases[3]=true,
        m=>m.FrameElements.Values.Single().Offset=new(.1,0,0),m=>m.FrameElements.Values.Single().Angle=.2,
        m=>m.FrameSections.Values.Single().J*=2,m=>m.ModelUnits.LengthUnit=ModelUnitsModel.LengthUnitTypes.MM})
    {var m=Cantilever();string original=ModelFingerprint.Compute(m);change(m);Assert(original!=ModelFingerprint.Compute(m));}
});
Test("Results cannot attach to a different input Model",()=>{var m=Cantilever();var results=SampleResults(m);m.NodeElements[50].Z=1;Throws(()=>m.AttachResults(results));});
Test("Results survive Model copy but not rebuild or merge",()=>{var m=Cantilever();m.AttachResults(SampleResults(m));var copy=new ModelModel(m);Assert(ReferenceEquals(copy.Results,m.Results));copy.Results.ValidateFor(copy);Assert(ModelComposer.Merge(new[]{m}).Results==null);m.BuildModel(new(){new NodeElementModel()});Assert(m.Results==null);});
Test("Query refuses stale results after input mutation",()=>{var m=Cantilever();m.AttachResults(SampleResults(m));m.NodeElements[50].NodalLoadList[0].FZ=-10;Throws(()=>m.Results.Select(m,ResultEntity.Node,ResultQuantity.Displacement).ToArray());});
Test("Result rows are immutable copies and invalid values fail",()=>{var values=new[]{new[]{1d,2}};var positions=new[]{0d};var columns=new[]{"A","B"};var r=new ElementResult(1,ResultEntity.Beam,1,1,ResultQuantity.Force,0,"Principal","Distance",columns,positions,values);values[0][0]=100;positions[0]=2;columns[0]="X";Near(r.Values[0][0],1);Near(r.Positions[0],0);Assert(r.Columns[0]=="A");Throws(()=>new ElementResult(1,ResultEntity.Beam,1,1,ResultQuantity.Force,0,"","",new[]{"A"},new[]{0d},new[]{new[]{double.NaN}}));});
Test("Missing result cases and provenance fail explicitly",()=>{var m=Cantilever();var r=SampleResults(m,false,true);m.AttachResults(r);Assert(!r.IsComplete&&!r.ProvenanceVerified);Throws(()=>r.ValidateFor(m,true));Throws(()=>r.ValidateFor(m,requireProvenance:true));Throws(()=>r.Select(m,ResultEntity.Node,ResultQuantity.Displacement,2).ToArray());Assert(!r.Select(m,ResultEntity.Node,ResultQuantity.Displacement,1,new[]{999}).Any());});
Test("Combinations enable their underlying cases even when direct cases are disabled",()=>{
    var a=new LoadCaseModel("G");var b=new LoadCaseModel("Q");var m=new ModelModel();m.BuildModel(new(){new NodeElementModel()},inputcombos:new(){new("ULS",new(){new(a,1.35),new(b,1.5)},LoadCombinationModel.LoadCombinationTypes.Linear)});
    var selection=AnalysisSelection.Create(m,null,new[]{"ULS"},false,true);Assert(selection.Cases.SequenceEqual(new[]{"G","Q"})&&selection.Combinations.SequenceEqual(new[]{"ULS"}));
    Throws(()=>AnalysisSelection.Create(m,new[]{"Typo"},null));Throws(()=>AnalysisSelection.Create(m,null,null,false,false));
    Assert(AnalysisSelection.Create(m,new[]{"G"},null,true,false).Cases.SequenceEqual(new[]{"G"}));
});
Test("Duplicate combination factors and selection retain every dependency",()=>{var m=Cantilever();m.LoadCombinations.Add(new LoadCombinationModel("C",new(){new(loadCase,1),new(loadCase,2)},LoadCombinationModel.LoadCombinationTypes.Linear));Assert(AnalysisSelection.Create(m,null,null,false,true).Cases.Length==1);string hash=ModelFingerprint.Compute(m);m.LoadCombinations["C"].LoadFactorList[1].Factor=3;Assert(hash!=ModelFingerprint.Compute(m));});
Test("Input/result manifest detects replacement of either file",()=>{
    string dir=Path.Combine(Path.GetTempPath(),"Rhino2Straus-proof-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
    string model=Path.Combine(dir,"test.st7"),result=Path.Combine(dir,"test.LSA");
    try { File.WriteAllText(model,"model");File.WriteAllText(result,"results");Assert(!ResultProvenance.Verify(model,result,"test"));ResultProvenance.Save(model,result,"test","LSA");Assert(ResultProvenance.Verify(model,result,"test"));File.WriteAllText(model,"changed");Assert(!ResultProvenance.Verify(model,result,"test"));File.WriteAllText(model,"model");File.WriteAllText(result,"changed");Assert(!ResultProvenance.Verify(model,result,"test")); }
    finally {File.Delete(model);File.Delete(result);File.Delete(ResultProvenance.ManifestPath(result));Directory.Delete(dir);}
});
Test("Invalid requested station counts fail before native file changes",()=>Throws(()=>ModelAnalysisService.RunStatic(Cantilever(),"unused.st7",false,stations:1)));
Test("Design strengths convert MPa metadata to native kPa without changing density",()=>{Near(MaterialModel.S355.YieldStressKPa,355000);Near(MaterialModel.C25_30.CompressiveStressKPa,25000);Near(MaterialModel.S355.Mass,MaterialModel.S355.Density);});
Test("Load case graph is canonical and copy retains IDs with mixed automatic nodes",()=>{
    var a=new NodeElementModel(0,0,0){Id=50};a.NodalLoadList.Add(new NodalLoadModel(new LoadCaseModel("Q"),1,0,0,0,0,0));a.Groups.Add(new ElementGroupModel("GQ"));
    var b=new NodeElementModel(1,0,0);b.NodalLoadList.Add(new NodalLoadModel(new LoadCaseModel("G"),0,1,0,0,0,0));b.Groups.Add(new ElementGroupModel("GG"));
    var m=new ModelModel();m.BuildModel(new(){a,b});m.LoadCases["Q"].GravityNoStructuralMass=true;
    Assert(ReferenceEquals(m.NodeElements[50].NodalLoadList[0].LoadCase,m.LoadCases["Q"]));
    var copy=new ModelModel(m);Assert(ModelFingerprint.Compute(m)==ModelFingerprint.Compute(copy));Assert(copy.LoadCases["Q"].GravityNoStructuralMass);Assert(m.LoadCases.Keys.SequenceEqual(copy.LoadCases.Keys));
});
Test("Integrated nonstatic solver options validate before export",()=>{Throws(()=>ModelAnalysisService.RunModal(Cantilever(),"unused.st7",false,0));Throws(()=>ModelAnalysisService.RunNonlinear(Cantilever(),"unused.st7",false,"Missing"));Throws(()=>ModelAnalysisService.RunNonlinear(Cantilever(),"unused.st7",false,"Tip load",10001));});


Test("Every added shape has a valid complete R31 parameter array",()=>{
    Assert(NativeSectionCatalog.All.Count==15&&NativeSectionCatalog.All.Count(s=>s.IsBgl)==6);
    foreach(var spec in NativeSectionCatalog.All){var d=new NativeSectionDefinition{IsBgl=spec.IsBgl,Shape=spec.Shape,Dimensions=spec.DefaultDimensions()};d.Validate();Assert(spec.Parameters.Select(p=>p.Index).Distinct().Count()==spec.Parameters.Length);}
});
Test("Native section copies isolate dimensions, units and meshing options",()=>{
    var a=new FrameSectionModel(section){NativeDefinition=new(){IsBgl=true,Shape=St7.bgISection,Dimensions=NativeSectionCatalog.BglI.DefaultDimensions()}};
    var b=new FrameSectionModel(a);b.NativeDefinition.Dimensions[12]=8;b.NativeDefinition.Options.Divisions=40;
    Near(a.NativeDefinition.Dimensions[12],0);Assert(a.NativeDefinition.Options.Divisions==24&&!a.Equals(b));
});
Test("Native section metadata survives Model copy and affects result identity",()=>{
    var m=Cantilever();var sec=m.FrameElements.Values.Single().FrameSection;sec.NativeDefinition=new(){Shape=St7.bsCruciform,Dimensions=NativeSectionCatalog.Cruciform.DefaultDimensions()};
    var c=new ModelModel(m);var copied=c.FrameElements.Values.Single().FrameSection.NativeDefinition;
    Assert(copied.Shape==St7.bsCruciform);var hash=ModelFingerprint.Compute(c);copied.Dimensions[0]*=2;Assert(hash!=ModelFingerprint.Compute(c));Near(sec.NativeDefinition.Dimensions[0],.2);
});
Test("Native section unit mismatch is rejected before export",()=>{
    var m=Cantilever();m.FrameElements.Values.Single().FrameSection.NativeDefinition=new(){Shape=St7.bsCruciform,Dimensions=NativeSectionCatalog.Cruciform.DefaultDimensions(),LengthUnit=ModelUnitsModel.LengthUnitTypes.MM};
    Assert(ModelValidator.Validate(m).Errors.Any(e=>e.Contains("different length units")));
});
Test("Native section invalid shape, array and options fail without API",()=>{
    Throws(()=>new NativeSectionDefinition{Shape=St7.bsBXSSection,Dimensions=new double[6]}.Validate());
    Throws(()=>new NativeSectionDefinition{Shape=St7.bsCruciform,Dimensions=new[]{double.NaN,1d,0,1,1,0}}.Validate());
    Throws(()=>new NativeSectionOptions{Divisions=0}.Validate());Throws(()=>new NativeSectionOptions{CircularDivisions=7}.Validate());
});
Test("All five link types retain options and canonical nodes in one Model",()=>{
    var links=Enum.GetValues<LinkPropertyModel.LinkPropertyTypes>().Select(t=>new LinkElementModel(new(0,0,0),new(1,0,0),new(t,100,200,300,10,20,30))).ToList();
    var m=new ModelModel();m.BuildModel(inputLinks:links);Assert(m.LinkElements.Count==5&&m.NodeElements.Count==2&&ModelValidator.Validate(m).IsValid);
    var copy=new ModelModel(m);foreach(var link in copy.LinkElements.Values){Assert(ReferenceEquals(link.NodeStart,copy.NodeElements[link.NodeStart.Id]));Assert(ReferenceEquals(link.NodeEnd,copy.NodeElements[link.NodeEnd.Id]));}
    var p=copy.LinkElements.Values.Single(x=>x.LinkProperty.Type==LinkPropertyModel.LinkPropertyTypes.MASTER_SLAVE).LinkProperty;
    p.Options.Dofs[0]=-1;Assert(m.LinkElements.Values.All(x=>x.LinkProperty.Options.Dofs[0]==1));
});
Test("Link clone and result identity include constraint options",()=>{
    var link=new LinkElementModel(new(0,0,0),new(1,0,0),new(LinkPropertyModel.LinkPropertyTypes.RIGID));var clone=new LinkElementModel(link);clone.LinkProperty.Options.RigidPlane=2;
    Assert(link.LinkProperty.Options.RigidPlane==0);var m=new ModelModel();m.BuildModel(inputLinks:new(){link});var hash=ModelFingerprint.Compute(m);m.LinkElements.Values.Single().LinkProperty.Options.RigidPlane=1;Assert(hash!=ModelFingerprint.Compute(m));
});
Test("Invalid and unconstrained master-slave settings are rejected",()=>{
    Throws(()=>new LinkConstraintOptions{RigidPlane=4}.Validate());Throws(()=>new LinkConstraintOptions{Dofs=new[]{1,1,1}}.Validate());Throws(()=>new LinkConstraintOptions{SymmetryAxis=0}.Validate());
    var m=new ModelModel();m.BuildModel(inputLinks:new(){new(new(0,0,0),new(1,0,0),new(LinkPropertyModel.LinkPropertyTypes.MASTER_SLAVE){Options=new(){Dofs=new int[6]}})});
    Assert(ModelValidator.Validate(m).Errors.Any(e=>e.Contains("no constrained DoF")));
});
Test("Generic centroids remain distinct for section offsets",()=>{var a=new FrameSectionModel(section);var b=new FrameSectionModel(section){Centroid=new Point2d(1,2)};Assert(!a.Equals(b));});


Test("Preview switches are independent and master-off hides every category",()=>{
    var display=new PreviewSettings{Layers=PreviewLayer.All};foreach(var layer in Enum.GetValues<PreviewLayer>().Where(v=>v is not PreviewLayer.All and not PreviewLayer.None))Assert(display.Shows(layer));
    Assert(!(display with{Enabled=false}).Shows(PreviewLayer.Loads));Assert(!(display with{Layers=display.Layers&~PreviewLayer.Sections}).Shows(PreviewLayer.Sections));Assert((display with{Layers=PreviewLayer.Loads}).Shows(PreviewLayer.Loads));
});
Test("Invalid display scales and transparency fail without rendering",()=>{
    Throws(()=>new PreviewSettings{SymbolScale=0}.Validate());Throws(()=>new PreviewSettings{LoadScale=double.NaN}.Validate());Throws(()=>new PreviewSettings{Transparency=1.1}.Validate());
});
Test("Beam interval conversion preserves full-span and partial linear loads",()=>{
    var load=new FrameLoadStrausModel{LoadSchema=FrameLoadStrausModel.LoadSchemas.Uniform,A=0,B=1,PA=5};
    var native=BeamLoadDistribution.NativeValues(load);Near(native[4],0);Near(native[5],0);Near(BeamLoadDistribution.Knots(load)[1].Position,1);
    load.LoadSchema=FrameLoadStrausModel.LoadSchemas.Keystone;load.A=.2;load.B=.7;load.PB=10;native=BeamLoadDistribution.NativeValues(load);Near(native[4],.2);Near(native[5],.3);Near(BeamLoadDistribution.Knots(load)[1].Value,10);
});
Test("Three-point and trapezoidal load diagrams retain native endpoint values",()=>{
    var load=new FrameLoadStrausModel{LoadSchema=FrameLoadStrausModel.LoadSchemas.TriangularEnds_Keystone,A=.2,B=.8,P1=1,PA=3,PB=4,P2=2};
    var points=BeamLoadDistribution.Knots(load);Assert(points.Select(p=>p.Position).SequenceEqual(new[]{0d,.2,.8,1}));Assert(points.Select(p=>p.Value).SequenceEqual(new[]{1d,3,4,2}));Near(BeamLoadDistribution.NativeValues(load)[5],.8);
    load.LoadSchema=FrameLoadStrausModel.LoadSchemas.Triangular;Assert(BeamLoadDistribution.Knots(load).Count==3);
});

Test("Imported coincident nodes retain topology, attributes and identity through copy",()=>{
    var a=new NodeElementModel(0,0,0){Id=1,TranslationMass=new(1,0,0)};
    var b=new NodeElementModel(0,0,0){Id=2,TranslationMass=new(2,0,0)};
    var c=new NodeElementModel(1,0,0){Id=3};a.Support.Dx=true;
    var source=new ModelImportData();source.Nodes.AddRange(new[]{a,b,c});source.Beams.Add(Beam(a,c));
    source.Links.Add(new LinkElementModel(a,b,new LinkPropertyModel(LinkPropertyModel.LinkPropertyTypes.GEN,100,100,100)));
    var result=source.Assemble(.001,false);Assert(result.Complete && result.Model!=null);
    var copy=new ModelModel(result.Model);Assert(copy.PreserveCoincidentNodes&&copy.NodeElements.Count==3);
    Near(copy.NodeElements[1].TranslationMass.X,1);Near(copy.NodeElements[2].TranslationMass.X,2);
    Assert(copy.LinkElements.Values.Single().NodeStart.Id!=copy.LinkElements.Values.Single().NodeEnd.Id);
    Assert(ReferenceEquals(copy.FrameElements.Values.Single().NodeStart,copy.NodeElements[1]));
    var merged=ModelComposer.Merge(new[]{copy,copy},true);Assert(merged.NodeElements.Count==6&&merged.PreserveCoincidentNodes);
});
Test("Partial import is explicit and repeated assembly does not accumulate issues",()=>{
    var source=new ModelImportData();source.Nodes.Add(new(0,0,0){Id=7});source.Issues.Add(new("Plate 8","Tri6 unsupported"));
    var strict=source.Assemble(.001,false);Assert(!strict.Complete&&strict.Model==null);
    var partial=source.Assemble(.001,true);Assert(!partial.Complete&&partial.Model.NodeElements.ContainsKey(7));
    Assert(source.Assemble(.001,true).Issues.Length==partial.Issues.Length);Throws(()=>source.Assemble(0,true));
});
Test("Import keeps unused case numbers and unknown material strengths",()=>{
    var source=new ModelImportData();source.Nodes.Add(new(0,0,0){Id=2});source.Cases.Add(new("Unused"){Id=9});
    var model=source.Assemble(.001,true).Model;Assert(model.LoadCases["Unused"].Id==9);Assert(new ModelModel(model).LoadCases["Unused"].Id==9);
    Assert(ModelComposer.Merge(new[]{model},false).LoadCases.ContainsKey("Unused"));
    var material=new MaterialModel{Type=MaterialModel.MaterialTypes.Isotropic};Near(material.TensileYeld,0);Near(material.CompressiveStrength,0);
});
Test("Triggered work runs once, invalidates on input changes and retains failure until rearmed",()=>{
    var operation=new TriggeredOperation<string>();int calls=0;
    string Work(){calls++;return "result";}
    operation.Update("a",false,Work,_=>"OK");Assert(calls==0);
    operation.Update("a",true,Work,_=>"OK");operation.Update("a",true,Work,_=>"OK");Assert(calls==1&&operation.Value=="result");
    operation.Update("b",true,Work,_=>"OK");Assert(calls==1&&operation.Value==null);
    operation.Update("b",false,Work,_=>"OK");Throws(()=>operation.Update("b",true,()=>throw new Exception("failure"),_=>"OK"));
    operation.Update("b",true,Work,_=>"OK");Assert(operation.Status=="Failed"&&operation.Value==null&&calls==1);
    operation.Update("b",false,Work,_=>"OK");operation.Update("b",true,Work,_=>"OK");Assert(calls==2);
});

Test("Thermal import metadata survives Model copies and invalidates results",()=>{
    var m=Cantilever();var lc=m.LoadCases.Values.Single();lc.ReferenceTemperature=20;
    m.NodeElements[50].Temperatures.Add(new(){LoadCase=lc,Temperature=35,Type=1});
    var copy=new ModelModel(m);Near(copy.LoadCases.Values.Single().ReferenceTemperature.Value,20);Assert(copy.NodeElements[50].Temperatures.Single().Type==1);
    string fingerprint=ModelFingerprint.Compute(copy);copy.LoadCases.Values.Single().ReferenceTemperature=25;Assert(ModelFingerprint.Compute(copy)!=fingerprint);
    Throws(()=>ModelComposer.Merge(new[]{m,copy},true));
    Assert(ModelValidator.Validate(copy).IsValid);copy.NodeElements[50].Temperatures[0].Type=3;Assert(!ModelValidator.Validate(copy).IsValid);
});

if (args.Contains("--native"))
{
    StrausRuntime.Configure(); // Install the resolver before the first direct vendor API call.
    string folder = Path.Combine(Path.GetTempPath(), "Rhino2Straus-tests-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(folder);
    Console.WriteLine("Native test files: " + folder);
    string path = Path.Combine(folder, "cantilever.st7");
    Test("Native export saves complete model", () => { Check(St7.St7SetLicenceOptions(St7.lmAbort,0,0)); var m=Cantilever();Assert(m.CreateModelStraus7R3(path,out _,out var warnings,out var errors),string.Join("; ",errors));Assert(File.Exists(path)); });
    if (File.Exists(path))
    {
        Test("Native export reports unsupported node groups without rejecting the model", () => {
            var m=Cantilever();m.NodeElements[10].Groups.Add(new ElementGroupModel("Node metadata"));
            string grouped=Path.Combine(folder,"node-groups.st7");
            Assert(m.CreateModelStraus7R3(grouped,out _,out var warnings,out var errors),string.Join("; ",errors));
            Assert(warnings.Any(w=>w.Contains("node groups")));
            Assert(StrausModelImport.Read(grouped,allowPartial:true).Model.NodeElements.Values.All(n=>n.Groups.Count==0));
        });
        Test("Native units, connectivity, IDs and load accumulation roundtrip",()=>{
            using var s=new StrausSession(path);var u=new int[6];Check(St7.St7GetUnits(s.Id,u));Assert(u[St7.ipFORCEU]==St7.fuKILONEWTON&&u[St7.ipLENGTHU]==St7.luMETRE);
            var f=new double[3];Check(St7.St7GetNodeForce3(s.Id,2,1,f));Near(f[2],-1);var c=new int[21];Check(St7.St7GetElementConnection(s.Id,St7.tyBEAM,1,c));Assert(c[0]==2&&c[1]==1&&c[2]==2);
            var data=new double[9];Check(St7.St7GetBeamMaterialData(s.Id,1,data));Near(data[St7.ipBeamDensity],7.849);
        });
        Test("Native geometry import preserves external labels",()=>{var g=StrausFileService.ReadGeometry(path);Assert(g.Nodes.Length==2&&g.Nodes[1].Id==50&&g.Beams.Length==1);Near(g.Nodes[1].Position.X,2);});
        Test("Native Model import keeps source bytes and returns editable structural data",()=>{
            var before=File.ReadAllBytes(path);var imported=StrausModelImport.Read(path);
            Assert(imported.Complete,string.Join("; ",imported.Issues.Select(i=>i.ToString())));Assert(imported.Model.NodeElements.Count==2&&imported.Model.FrameElements.Count==1);
            Assert(before.SequenceEqual(File.ReadAllBytes(path)));Assert(imported.Identities.Any(i=>i.Entity=="Node"&&i.NativeLabel==50));
            Near(imported.Model.NodeElements[2].NodalLoadList.Sum(l=>l.FZ),-1);Near(imported.Model.Materials.Values.Single().ModulusElasticity,2.1e8);
        });
        Test("Existing files are protected",()=>{var before=File.ReadAllBytes(path);Assert(!Cantilever().CreateModelStraus7R3(path,out _,out _,out _));Assert(File.ReadAllBytes(path).SequenceEqual(before));});
        Test("Native export adds thermal loads, releases, springs and nodal mass",()=>{
            var m=Cantilever();m.NodeElements[50].TranslationMass=new Vector3d(1,2,3);m.NodeElements[10].TranslationSpring=new Vector3d(100,200,300);m.NodeElements[50].Temperatures.Add(new NodeTemperatureModel{LoadCase=loadCase,Temperature=25});m.FrameElements.Values.Single().EndReleases[3]=true;
            string p=Path.Combine(folder,"attributes.st7");Assert(m.CreateModelStraus7R3(p,out _,out _,out var e),string.Join(";",e));using var s=new StrausSession(p);var mass=new double[3];Check(St7.St7GetNodeTMass3(s.Id,2,mass));Near(mass[2],3);var temp=new double[1];Check(St7.St7GetNodeTemperature1(s.Id,2,1,temp));Near(temp[0],25);var status=new int[3];Check(St7.St7GetBeamRRelease3(s.Id,1,2,status,new double[3]));Assert(status[0]==St7.brReleased);
        });
        if (args.Contains("--solve")) Test("Native cantilever solver, displacement, reactions and beam forces",()=>{
            string result=StrausFileService.RunLinearStatic(path,false);Assert(File.Exists(result));var d=StrausFileService.NodeResults(path,result,1,new[]{2},false);// Timoshenko cantilever: bending plus shear, P=-1 kN, L=2 m.
            double expected = -(Math.Pow(2, 3) / (3 * material.ModulusElasticity * section.I11)
                + 2 / (material.ShearModulus * section.ShearA1));
            Near(d[0][2],expected,Math.Abs(expected)*1e-6);
            var reaction=StrausFileService.NodeResults(path,result,1,new[]{1},true);Near(reaction[0][2],1,1e-6);var b=StrausFileService.BeamResults(path,result,1,1,St7.rtBeamForce,St7.stBeamPrincipal,5);Assert(b.Values.Length>=5);Assert(StrausFileService.ResultCases(path,result).Length>=1);
        });
        if (args.Contains("--solve")) Test("Integrated Model solver generates selected combinations and attaches verified results",()=>{
            var m=Cantilever();var combo=new LoadCombinationModel("Double",new(){new(loadCase,2)},LoadCombinationModel.LoadCombinationTypes.Linear);
            m.BuildModel(m.NodeElements.Values.ToList(),m.FrameElements.Values.ToList(),inputcombos:new(){combo});
            var solved=ModelAnalysisService.RunStatic(m,Path.Combine(folder,"integrated.st7"),false,runCases:false,combinations:new[]{"Double"});
            solved.Results.ValidateFor(solved,requireProvenance:true);Assert(solved.Results.Cases.Any(c=>c.Secondary));Assert(solved.Results.BeamSections.Count==1);
            var displacements=solved.Results.Select(solved,ResultEntity.Node,ResultQuantity.Displacement,ids:new[]{50}).ToArray();
            Assert(displacements.Length>=2);double baseValue=displacements.Single(x=>!solved.Results.Cases.Single(c=>c.Number==x.CaseNumber).Secondary).Values[0][2];
            double comboValue=displacements.Single(x=>solved.Results.Cases.Single(c=>c.Number==x.CaseNumber).Secondary).Values[0][2];Near(comboValue,2*baseValue,1e-8);
            var reread=ModelAnalysisService.Read(solved,solved.Results.ModelPath,solved.Results.ResultPath);Assert(reread.Results.ProvenanceVerified);
        });
    }
}
Console.WriteLine($"{passed} passed; {failures} failed.");
return failures == 0 ? 0 : 1;

    }
}
