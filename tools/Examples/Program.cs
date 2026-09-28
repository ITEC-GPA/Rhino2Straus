using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;
using Rhino.Runtime.InProcess;
using Rhino.Geometry;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Models;
using Rhino2Straus.Core.Settings;
using Rhino2Straus.Grasshopper.Datatype;

internal static class Program
{
    [STAThread] static int Main(string[] args)
    {
        RhinoInside.Resolver.Initialize(args.FirstOrDefault(a=>a.StartsWith("--rhino="))?.Substring(8)??@"C:\Program Files\Rhino 8\System");
        return Start(args);
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static int Start(string[] args)
    {
        try
        {
            Console.WriteLine("Starting isolated Rhino / Grasshopper runtime...");
            using var rhino=new RhinoCore(new[]{"/nosplash","/safemode","/notemplate"},WindowStyle.NoWindow);
            var plugin=Assembly.LoadFrom(Path.GetFullPath("Rhino2Straus.Grasshopper/bin/Release/net8.0-windows/Rhino2Straus.Grasshopper.gha"));
            RegisterComponents(plugin);
            GH_Document.EnableSolutions=true;
            string output="examples";Directory.CreateDirectory(output);
            var reports=new List<object>();
            foreach(var make in new Func<Assembly,ExampleDocument>[]{Definitions.Cantilever,Definitions.Portal,Definitions.Slab})
            {
                using var example=make(plugin);
                Console.WriteLine("Solving "+example.FileName);
                var initial=Validate(example.Document,example);
                var files=new List<object>();
                foreach(string extension in new[]{".gh",".ghx"})
                {
                    string path=Path.Combine(output,example.FileName+extension);
                    var archive=new GH_Archive();
                    if(!archive.AppendObject(example.Document,"Definition")||!archive.WriteToFile(path,true,false))throw new Exception("Cannot save "+path);
                    if(extension==".ghx")File.WriteAllText(path,File.ReadAllText(path).Replace("\r\n","\n"));
                    using var restored=new GH_Document();var read=new GH_Archive();
                    if(!read.ReadFromFile(path)||!read.ExtractObject(restored,"Definition"))throw new Exception("Cannot restore "+path);
                    Require(restored.ObjectCount==example.Document.ObjectCount,"Object count changed on reload");
                    Require(Connections(restored)==Connections(example.Document),"Connections changed on reload");
                    Validate(restored,example);
                    files.Add(new{file=Path.GetFileName(path),bytes=new FileInfo(path).Length,sha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant()});
                    Console.WriteLine("PASS save / reopen / solve "+path);
                }
                reports.Add(new{example=example.FileName,components=example.Document.Objects.OfType<GH_Component>().Count(),pluginComponents=example.Document.Objects.OfType<GH_Component>().Count(c=>c.GetType().Assembly==plugin),connections=Connections(example.Document),validation=initial,files});
            }
            File.WriteAllText(Path.Combine(output,"validation.json"),JsonSerializer.Serialize(new{rhino=Rhino.RhinoApp.Version.ToString(),plugin=plugin.GetName().Version.ToString(),nativeSolverRun=false,reports},new JsonSerializerOptions{WriteIndented=true})+"\n");
            Console.WriteLine("PASS all three definitions, both formats, connected graph recomputation, Model preflight and closed Breps. Native solver not invoked.");
            return 0;
        }
        catch(Exception e){Console.WriteLine(e);return 1;}
    }
    private static void RegisterComponents(Assembly assembly)
    {
        // The headless generator must never scan or load the user's installed third-party GH plugins.
        // These private APIs are isolated here; fail explicitly if a future GH changes the registry.
        var field=typeof(Instances).GetField("m_comServer",BindingFlags.NonPublic|BindingFlags.Static)
            ??throw new NotSupportedException("This Grasshopper version has no isolated component registry field.");
        field.SetValue(null,new GH_ComponentServer());
        var info=(GH_AssemblyInfo)Activator.CreateInstance(assembly.GetTypes().Single(t=>!t.IsAbstract&&typeof(GH_AssemblyInfo).IsAssignableFrom(t)));
        Instances.ComponentServer.AddProxyLibraryInfo(info);
        var proxy=typeof(GH_Component).Assembly.GetType("Grasshopper.Kernel.GH_CompiledObjectProxy")
            ?.GetConstructor(BindingFlags.NonPublic|BindingFlags.Instance,null,new[]{typeof(IGH_DocumentObject),typeof(GH_AssemblyInfo)},null)
            ??throw new NotSupportedException("This Grasshopper version cannot register isolated object proxies.");
        var builtins=new[]{typeof(Param_Point),typeof(Param_Curve),typeof(Param_Mesh),typeof(Param_Vector),typeof(Param_Number),typeof(Param_Integer),typeof(Param_String),typeof(Param_Brep),typeof(GH_Panel),typeof(GH_Group),typeof(GH_BooleanToggle),typeof(GH_NumberSlider),typeof(Grasshopper.Kernel.Components.GH_StreamGateComponent)};
        foreach(var type in assembly.GetTypes().Where(t=>!t.IsAbstract&&typeof(GH_Component).IsAssignableFrom(t)).Concat(builtins))
        {
            var obj=(IGH_DocumentObject)Activator.CreateInstance(type);
            Instances.ComponentServer.AddProxy((IGH_ObjectProxy)proxy.Invoke(new object[]{obj,info}));
        }
    }
    private static int Connections(GH_Document d)=>d.Objects.Sum(o=>o is GH_Component c?c.Params.Input.Sum(p=>p.SourceCount):o is IGH_Param p?p.SourceCount:0);
    private static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
    private static object Validate(GH_Document document,ExampleDocument specification)
    {
        // Check Run before solving a loaded document: regeneration never contacts the Straus API.
        foreach(var solve in document.Objects.OfType<GH_Component>().Where(c=>c.GetType().Name=="RunModelCasesComponent"))
        {
            foreach(string name in new[]{"Run","Overwrite"})
            {
                var sources=ExampleDocument.Input(solve,name).Sources;
                Require(sources.Count==1&&sources[0] is GH_BooleanToggle toggle&&!toggle.Value,name+" must remain wired to a false toggle");
            }
        }
        document.Enabled=true;
        document.NewSolution(true,GH_SolutionMode.Silent);
        var errors=document.Objects.OfType<IGH_ActiveObject>().SelectMany(c=>c.RuntimeMessages(GH_RuntimeMessageLevel.Error).Select(m=>c.Name+": "+m)).ToArray();
        Require(errors.Length==0,string.Join("\n",errors));
        var final=(GH_Component)document.FindObject(specification.FinalModel.InstanceGuid,false);
        var model=ExampleDocument.Output(final,"Model").VolatileData.AllData(true).OfType<GH_Model>().Single().Value;
        var counts=new[]{model.NodeElements.Count,model.FrameElements.Count,model.AreaElements.Count,model.LoadCases.Count,model.LoadCombinations.Count};
        Require(counts.SequenceEqual(specification.ExpectedCounts),"Unexpected model counts: "+string.Join(",",counts));
        var preflight=ModelValidator.Validate(model);Require(preflight.IsValid,string.Join("\n",preflight.Errors));
        Require(model.Results==null,"Examples must ship without synthetic or cached solver results");
        var geometry=new ModelBrepGeometry(model);
        Require(geometry.Issues.Count==0,string.Join("\n",geometry.Issues));
        int physicalCount=counts[1]+counts[2];Require(geometry.Elements.Count==physicalCount,"Physical Brep count");
        Require(geometry.Elements.All(e=>e.Geometry.IsValid&&e.Geometry.IsSolid),"Every Brep must be valid and closed");
        double volume=geometry.Elements.Sum(e=>{using var v=VolumeMassProperties.Compute(e.Geometry);return v.Volume;});
        Require(Math.Abs(volume-specification.ExpectedVolume)<1e-7,"Unexpected volume "+volume);
        var manager=document.Objects.OfType<GH_Component>().Single(c=>c.GetType().Name=="PreviewModelComponent");
        Require(ExampleDocument.Output(manager,"Breps").VolatileDataCount==physicalCount,"Connected manager Brep output");
        var brepParam=document.Objects.OfType<Param_Brep>().Single();Require(brepParam.VolatileDataCount==physicalCount,"Downstream native Brep parameter");
        string fingerprint=ModelFingerprint.Compute(model);
        var displaySwitch=document.Objects.OfType<GH_BooleanToggle>().Single(t=>t.NickName=="Preview ON/OFF");
        Require(displaySwitch.Value,"Examples open with preview enabled");
        displaySwitch.Value=false;displaySwitch.ExpireSolution(false);document.NewSolution(false,GH_SolutionMode.Silent);
        var effective=(PreviewSettings)ExampleDocument.Output(manager,"Settings").VolatileData.AllData(true).Single().ScriptVariable();
        Require(!effective.Enabled,"Preview toggle does not reach the manager");
        Require(ExampleDocument.Output(manager,"Status").VolatileData.AllData(true).OfType<GH_String>().Single().Value.Contains("OFF"),"Manager preview must be OFF");
        Require(brepParam.VolatileDataCount==physicalCount,"Brep output must survive preview OFF");
        var after=ExampleDocument.Output(final,"Model").VolatileData.AllData(true).OfType<GH_Model>().Single().Value;
        Require(ModelFingerprint.Compute(after)==fingerprint,"Preview changed model data");
        displaySwitch.Value=true;displaySwitch.ExpireSolution(false);document.NewSolution(false,GH_SolutionMode.Silent);
        var warnings=document.Objects.OfType<IGH_ActiveObject>().SelectMany(c=>c.RuntimeMessages(GH_RuntimeMessageLevel.Warning).Select(m=>c.Name+": "+m)).ToArray();
        Console.WriteLine($"Model {string.Join("/",counts)}; {physicalCount} closed Breps; volume {volume:G6}; warnings {warnings.Length}");
        return new{nodes=counts[0],beams=counts[1],plates=counts[2],loadCases=counts[3],combinations=counts[4],closedBreps=physicalCount,volume,previewToggleVerified=true,preflightErrors=preflight.Errors,preflightWarnings=preflight.Warnings,runtimeErrors=errors,runtimeWarnings=warnings};
    }
}
