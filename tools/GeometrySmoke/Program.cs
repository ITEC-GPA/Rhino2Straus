using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Grasshopper.Kernel;
using System.Runtime.InteropServices;
using Rhino.Geometry;
using Rhino.Runtime.InProcess;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Models;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;

internal static class Program
{
    [STAThread] static int Main(string[] args)
    {
        RhinoInside.Resolver.Initialize(args.Length>0?args[0]:@"C:\Program Files\Rhino 8\System");
        return StartRhino();
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static int StartRhino()
    {
        try
        {
            using var rhino=new RhinoCore(new[]{"/nosplash"},WindowStyle.NoWindow);
            Run();return 0;
        }
        catch(Exception e){Console.WriteLine(e);return 1;}
    }
    private static void Run()
    {
        int passed=0;
        void Check(bool condition,string name){if(!condition)throw new Exception(name);passed++;Console.WriteLine("PASS "+name);}
        var section=new FrameSectionModel("R",FrameSectionModel.SectionGeometryTypes.SB,FrameSectionModel.Types.DBUSER,FrameSectionModel.OffsetTypes.CC,.4,.2);
        Check(Math.Abs(section.SectionArea-.08)<1e-8,"Rectangle cross-sectional area");
        Check(Math.Abs(section.Ixy)<1e-10,"Symmetric section product of inertia is zero");
        var b=new FrameElementModel(new(0,0,0),new(2,0,0),new FramePropertyModel("R",MaterialModel.S355,section),0,new Vector3d(.3,.4,0));
        var breps=PreviewGeometry.Beam(b);Check(breps.Count==1&&breps[0].IsSolid,"Beam preview is capped solid");
        var volume=VolumeMassProperties.Compute(breps[0]);Console.WriteLine($"Beam volume={volume.Volume}; orientation={breps[0].SolidOrientation}; centroid={volume.Centroid}");Check(Math.Abs(volume.Volume-.16)<1e-6,"Beam preview volume");
        b.GetLocalAxes(true,out _,out var y,out var x,out _);var expected=new Point3d(1,0,0)+x*.3+y*.4;
        Check(volume.Centroid.DistanceTo(expected)<1e-7,"Beam preview includes principal offsets");
        b.FrameSection=new FrameSectionModel(section){Offset=FrameSectionModel.OffsetTypes.LT};
        var shifted=b.GetEffectiveOffset();Check(shifted.X!=.3||shifted.Y!=.4,"Section insertion contributes to exported offset");
        var rotated=ElementTransforms.Beam(b,Transform.Rotation(Math.PI/2,Vector3d.ZAxis,Point3d.Origin));
        Check(rotated.NodeEnd.Position.DistanceTo(new Point3d(0,2,0))<1e-8,"Rigid placement rotates beam connectivity");
        var plate=new AreaElementModel(new(){new(0,0,0),new(2,0,0),new(2,1,0),new(0,1,0)},new AreaPropertyModel("P",MaterialModel.S355,new AreaThicknessModel("t",.2,.2,.3),AreaPropertyModel.PlatePropertyType.PlateShell),0,.4);
        var mesh=PreviewGeometry.Plate(plate);Check(mesh.IsClosed,"Plate preview is watertight");
        var bounds=mesh.GetBoundingBox(true);Check(Math.Abs(bounds.Min.Z-.6)<1e-6&&Math.Abs(bounds.Max.Z-.8)<1e-6,"Plate element and property offsets are combined");
        foreach(var shape in new[]{FrameSectionModel.SectionGeometryTypes.SR,FrameSectionModel.SectionGeometryTypes.P,FrameSectionModel.SectionGeometryTypes.B,FrameSectionModel.SectionGeometryTypes.H,FrameSectionModel.SectionGeometryTypes.T,FrameSectionModel.SectionGeometryTypes.C,FrameSectionModel.SectionGeometryTypes.L})
        {
            var profile=new FrameSectionModel(shape.ToString(),shape,FrameSectionModel.Types.DBUSER,FrameSectionModel.OffsetTypes.CC,.4,shape==FrameSectionModel.SectionGeometryTypes.P?.02:.2,.02,.03,.2,.03);
            Check(profile.SectionArea>0&&profile.Breps?.Count>0,$"{shape} section geometry");
            Check(Math.Abs(profile.I11*profile.I22-(profile.Ixx*profile.Iyy-profile.Ixy*profile.Ixy))<1e-12,$"{shape} principal section inertia");
        }

        foreach(var end in new[]{new Point3d(2,0,0),new Point3d(0,0,2),new Point3d(0,0,-2),new Point3d(2,3,4)})
        {
            var beam=new FrameElementModel(new(0,0,0),new NodeElementModel(end),new FramePropertyModel("R",MaterialModel.S355,section),0,Vector3d.Zero);
            var direction=new Vector3d(1,2,3);var z=end-Point3d.Origin;z.Unitize();var target=direction-z*(direction*z);target.Unitize();
            var aligned=ElementAlignment.Beam(beam,direction);aligned.GetLocalAxes(true,out _,out _,out var xAligned,out _);
            Check(xAligned*target>1-1e-10,"Beam principal alignment "+end);
            var second=ElementAlignment.Beam(beam,direction,2);second.GetLocalAxes(true,out _,out var yAligned,out _,out _);
            Check(yAligned*target>1-1e-10,"Beam second axis alignment "+end);
            Check(beam.Angle==0,"Alignment leaves input beam unchanged "+end);
        }
        var oriented=ElementAlignment.Plate(plate,new Vector3d(1,2,5));oriented.GetLocalAxes(out var px,out _,out var pz);
        var targetPlate=new Vector3d(1,2,0);targetPlate.Unitize();Check(px*targetPlate>1-1e-10&&pz.Z>.999,"Plate axis projection preserves normal");
        bool rejected=false;try{ElementAlignment.Plate(plate,Vector3d.ZAxis);}catch(ArgumentException){rejected=true;}Check(rejected,"Parallel plate alignment direction is rejected");
        rejected=false;try{ElementAlignment.Beam(b,Vector3d.XAxis);}catch(ArgumentException){rejected=true;}Check(rejected,"Parallel beam alignment direction is rejected");
        var geometric=new FrameSectionModel(section){Angle=.3};var geometricBeam=new FrameElementModel(new(0,0,0),new(2,0,0),new FramePropertyModel("R",MaterialModel.S355,geometric),0,Vector3d.Zero);
        var geometricAligned=ElementAlignment.Beam(geometricBeam,Vector3d.ZAxis,1,false);geometricAligned.GetLocalAxes(false,out _,out _,out var localX,out _);Check(localX.Z>.999999,"Geometric section alignment accounts for principal section angle");

        var duplicate=new FrameSectionModel(section);duplicate.Breps[0].Translate(5,0,0);
        Check(Math.Abs(section.Breps[0].GetBoundingBox(true).Center.X)<1e-9,"Section preview copies do not mutate the original");
        bool invalid=false;try{new FrameSectionModel("bad pipe",FrameSectionModel.SectionGeometryTypes.P,FrameSectionModel.Types.DBUSER,FrameSectionModel.OffsetTypes.CC,.1,.1);}catch(ArgumentException){invalid=true;}
        Check(invalid,"Invalid pipe thickness is rejected before geometry construction");
        var insertionSection=new FrameSectionModel(section){Offset=FrameSectionModel.OffsetTypes.LT};
        var insertionModel=new ModelModel();insertionModel.BuildModel(inputFrames:new(){new(new(0,0,0),new(1,0,0),new FramePropertyModel("Insertion",MaterialModel.S355,insertionSection),0,Vector3d.Zero)});
        var insertionHash=ModelFingerprint.Compute(insertionModel);insertionModel.FrameElements.Values.Single().FrameSection.Breps[0].Scale(2);
        Check(insertionHash!=ModelFingerprint.Compute(insertionModel),"Profile-derived insertion changes invalidate results");
        Console.WriteLine($"{passed} geometry checks passed.");
        var directory=new DirectoryInfo(Environment.CurrentDirectory);
        while(directory!=null&&!File.Exists(Path.Combine(directory.FullName,"Rhino2Straus.sln")))
        {
            if(File.Exists(Path.Combine(directory.FullName,"Rhino2Straus","Rhino2Straus.sln"))){directory=new DirectoryInfo(Path.Combine(directory.FullName,"Rhino2Straus"));break;}
            directory=directory.Parent;
        }
        if(directory==null)throw new DirectoryNotFoundException("Run from the repository.");
        string plugin=Path.Combine(directory.FullName,"Rhino2Straus.Grasshopper","bin","Release","net8.0-windows","Rhino2Straus.Grasshopper.gha");
        string pluginFolder=Path.GetDirectoryName(plugin);
        System.Runtime.Loader.AssemblyLoadContext.Default.Resolving+=(context,name)=>{
            string candidate=Path.Combine(pluginFolder,name.Name+".dll");return File.Exists(candidate)?context.LoadFromAssemblyPath(candidate):null;
        };
        var assembly=Assembly.LoadFrom(plugin);var ids=new System.Collections.Generic.HashSet<Guid>();int count=0;
        foreach(var type in assembly.GetTypes().Where(t=>!t.IsAbstract&&typeof(GH_Component).IsAssignableFrom(t)))
        {
            var component=(GH_Component)Activator.CreateInstance(type);
            if(!ids.Add(component.ComponentGuid))throw new Exception("Duplicate GUID: "+component.Name);
            if(component.Icon_24x24==null||component.Icon_24x24.Width!=24||component.Icon_24x24.Height!=24)throw new Exception("Missing icon: "+component.Name);
            if(component.Params.Output.Count==0)throw new Exception("Missing output: "+component.Name);
            count++;
        }
        Console.WriteLine($"{count} Grasshopper components constructed: GUIDs, icons and registered outputs valid.");
        var model=new ModelModel();model.BuildModel(inputFrames:new(){new FrameElementModel(new(0,0,0),new(2,0,0),new FramePropertyModel("R",MaterialModel.S355,section),0,Vector3d.Zero)});
        var records=new System.Collections.Generic.List<ElementResult>();
        records.Add(new(1,ResultEntity.Beam,1,1,ResultQuantity.Force,0,"Principal 1,2,3","Distance",new[]{"V1","M1","V2","M2","N","T"},new[]{0d,2},new[]{new[]{10d,20,30,40,50,60},new[]{-10d,-20,-30,-40,-50,-60}}));
        foreach(var node in model.NodeElements.Values)records.Add(new(1,ResultEntity.Node,node.Id,node.Id,ResultQuantity.Displacement,0,"Global XYZ","Node",new[]{"UX","UY","UZ","RX","RY","RZ"},new[]{0d},new[]{new[]{0d,0,.01*node.Id,0,0,0}}));
        model.AttachResults(new(ModelFingerprint.Compute(model),"test.st7","test.LSA","Synthetic fixture",true,new[]{new ResultCaseInfo(1,"Test",false)},records,Array.Empty<ResultIssue>()));
        var types=Assembly.LoadFrom(Path.Combine(pluginFolder,"Rhino2Straus.Grasshopper.Types.dll"));
        var gooType=types.GetType("Rhino2Straus.Grasshopper.Datatype.GH_Model",true);
        var documents=new System.Collections.Generic.List<GH_Document>();
        GH_Component Solve(string className)
        {
            var type=assembly.GetTypes().Single(t=>t.Name==className);var component=(GH_Component)Activator.CreateInstance(type);
            var document=new GH_Document();documents.Add(document);document.AddObject(component,false);
            ((Param_GenericObject)component.Params.Input[0]).PersistentData.Append((IGH_Goo)Activator.CreateInstance(gooType,model));
            component.CollectData();component.ComputeData();
            var errors=component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
            Console.WriteLine($"{className}: phase={component.Phase}, output={component.Params.Output[0].VolatileDataCount}");
            if(errors.Count>0)throw new Exception(className+": "+string.Join("; ",errors));
            return component;
        }
        var actions=Solve("ModelBeamActionsComponent");
        Check(actions.Params.Output[0].VolatileData.AllData(true).OfType<GH_Number>().Select(v=>v.Value).SequenceEqual(new[]{50d,-50}),"Grasshopper beam N output follows API column mapping");
        Check(actions.Params.Output[5].VolatileData.AllData(true).OfType<GH_Number>().Select(v=>v.Value).SequenceEqual(new[]{40d,-40}),"Grasshopper beam M2 output follows API column mapping");
        var extrema=Solve("ModelResultEnvelopeComponent");
        Check(extrema.Params.Output[0].VolatileData.AllData(true).OfType<GH_Number>().Select(v=>v.Value).SequenceEqual(new[]{-50d,50}),"Grasshopper extrema preserve signed values");
        Check(extrema.Params.Output[2].VolatileData.AllData(true).OfType<GH_Number>().Select(v=>v.Value).SequenceEqual(new[]{2d,0}),"Grasshopper extrema return governing sample positions");
        var decompose=Solve("DecomposeModelComponent");Check(decompose.Params.Output[0].VolatileDataCount==2&&decompose.Params.Output[13].VolatileDataCount==1,"Grasshopper decomposes one Model with results");
        var displacements=Solve("ModelNodeDisplacementComponent");Check(displacements.Params.Output[0].VolatileDataCount==12,"Grasshopper nodal result trees retain six components per node");
        var verification=Solve("ModelVerificationDataComponent");Check(verification.Params.Output[3].VolatileDataCount==3,"Grasshopper verification data joins elements and result tables");
        var deformed=Solve("ModelDeformedGeometryComponent");Check(deformed.Params.Output[0].VolatileDataCount==2&&deformed.Params.Output[1].VolatileDataCount==1,"Grasshopper deformed geometry uses Model results");

        GH_Component SolveDefaults(string className,Action<GH_Component> setup=null)
        {
            var component=(GH_Component)Activator.CreateInstance(assembly.GetTypes().Single(t=>t.Name==className));
            var document=new GH_Document();documents.Add(document);document.AddObject(component,false);setup?.Invoke(component);
            component.CollectData();component.ComputeData();
            Check(component.RuntimeMessages(GH_RuntimeMessageLevel.Error).Count==0&&component.Params.Output[0].VolatileDataCount>0,"Grasshopper defaults solve "+className);
            return component;
        }
        foreach(string className in new[]{"FrameSectionAngleComponent","FrameSectionBoxComponent","FrameSectionChannelComponent","FrameSectionCircleComponent","FrameSectionIComponent","FrameSectionPipeComponent","FrameSectionRectangularComponent","FrameSectionTeeComponent","FrameSectionGenericComponent","MaterialComponent","DatabaseSteelMaterialComponent","DatabaseConcreteMaterialComponent","LoadCaseComponent","AreaThicknessComponent","LinkPropertyComponent","NativeSectionOptionsComponent","RigidLinkPropertyComponent","PinnedLinkPropertyComponent","MasterSlaveLinkPropertyComponent","SectorSymmetryLinkPropertyComponent","ElasticLinkPropertyComponent","LinkConstraintOptionsComponent"})SolveDefaults(className);
        var constraintOptions=SolveDefaults("LinkConstraintOptionsComponent",c=>{var input=(Param_Integer)c.Params.Input[2];input.PersistentData.Clear();input.PersistentData.Append(new GH_Integer(-1));});
        var master=SolveDefaults("MasterSlaveLinkPropertyComponent",c=>c.Params.Input[0].AddSource(constraintOptions.Params.Output[0]));
        var property=master.Params.Output[0].VolatileData.AllData(true).First().ScriptVariable();
        Check(property is LinkPropertyModel lp&&lp.Options.Dofs[0]==-1,"Link options travel through actual Grasshopper connections");
        var defaultLink=SolveDefaults("LinkElementComponent",c=>{
            ((Param_Point)c.Params.Input[0]).PersistentData.Append(new GH_Point(Point3d.Origin));
            ((Param_Point)c.Params.Input[1]).PersistentData.Append(new GH_Point(new Point3d(1,0,0)));
        });
        Check(defaultLink.Params.Output[0].VolatileData.AllData(true).First().ScriptVariable() is LinkElementModel dl&&dl.LinkProperty.Type==LinkPropertyModel.LinkPropertyTypes.RIGID,"Link element defaults to a rigid connection");
        Console.WriteLine($"{passed} geometry and Grasshopper data checks passed.");
        foreach(var document in documents)document.Dispose();
    }
}
