using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Rhino.Geometry;
using Rhino2Straus.Api;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Models;
using St7API;

namespace Rhino2Straus.Import;

/// <summary>Reads the supported elastic structural model from an isolated copy; the input file is never saved.</summary>
public static class StrausModelImport
{
    public static ModelImportResult Read(string path, int freedomCase = 1, bool allowPartial = false, double tolerance = .001)
    {
        path = Path.GetFullPath(path);
        if (!File.Exists(path) || !Path.GetExtension(path).Equals(".st7", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Choose an existing .st7 file.");
        if (freedomCase < 1 || !double.IsFinite(tolerance) || tolerance <= 0) throw new ArgumentException("Use a positive freedom case and tolerance.");
        string copy = Path.Combine(Path.GetTempPath(), "Rhino2Straus-import-" + Guid.NewGuid().ToString("N") + ".st7");
        try
        {
            File.Copy(path, copy);
            using var session = new StrausSession(copy, readOnly: false);
            // Convert values, not just unit labels. This also handles imperial files and non-kPa stress units.
            StrausRuntime.Check(St7.St7ConvertUnits(session.Id, StrausUnits.ToApi(new())), "Convert import copy to m, kN, kPa, tonne, C");
            return new NativeModelReader(session.Id, freedomCase).Read().Assemble(tolerance, allowPartial);
        }
        finally { if (File.Exists(copy)) File.Delete(copy); }
    }
}

internal sealed partial class NativeModelReader
{
    private readonly int id, freedom;
    private readonly ModelImportData data = new();
    private readonly Dictionary<int, NodeElementModel> nodes = new();
    private readonly Dictionary<int, LoadCaseModel> cases = new();
    private readonly Dictionary<int, ElementGroupModel> groups = new();
    private readonly Dictionary<int, FramePropertyModel> beamProperties = new();
    private readonly Dictionary<int, AreaPropertyModel> plateProperties = new();
    private readonly Dictionary<int, MaterialModel> solidMaterials = new();
    private int materialId;
    internal NativeModelReader(int id, int freedom) { this.id=id; this.freedom=freedom; }
    private static void Check(int code, string operation) => StrausRuntime.Check(code, operation);
    private void Omit(string owner, string message) => data.Issues.Add(new(owner,message));
    private static Vector3d Vector(double[] d, int start=0) => new(d[start],d[start+1],d[start+2]);
    private int Total(int entity) { int count=0;Check(St7.St7GetTotal(id,entity,ref count),"Read entity count");return count; }
    private double[] Values(int size, Func<double[],int> read, string operation)
    { var values=new double[size];Check(read(values),operation);if(values.Any(v=>!double.IsFinite(v)))throw new InvalidDataException(operation+": non-finite value");return values; }
    private string PropertyName(int entity,int property)
    { var name=new StringBuilder(1024);Check(St7.St7GetPropertyName(id,entity,property,name,name.Capacity),"Read property name");return $"{name} [{entity}:{property}]"; }
    private List<ElementGroupModel> Group(int entity,int number)
    {
        int group=0;Check(St7.St7GetEntityGroup(id,entity,number,ref group),"Read entity group");
        if(group<=1)return new();
        if(!groups.TryGetValue(group,out var result))
        {
            result=ReadGroup(group,new HashSet<int>());groups[group]=result;
        }
        return new(){result};
    }
    private ElementGroupModel ReadGroup(int number,HashSet<int> visiting)
    {
        if(groups.TryGetValue(number,out var result))return result;
        if(!visiting.Add(number))throw new InvalidDataException("Cyclic group hierarchy");
        var name=new StringBuilder(1024);int parent=0;
        Check(St7.St7GetGroupIDName(id,number,name,name.Capacity),"Read group name");Check(St7.St7GetGroupParent(id,number,ref parent),"Read group parent");
        result=new ElementGroupModel(name.ToString(),parent>1?ReadGroup(parent,visiting):null){Id=number};groups[number]=result;return result;
    }
    private (int Property,NodeElementModel[] Nodes) Connection(int entity,int number)
    {
        var c=new int[St7.kMaxElementNode+1];int property=0;
        Check(St7.St7GetElementConnection(id,entity,number,c),"Read connectivity");Check(St7.St7GetElementProperty(id,entity,number,ref property),"Read element property");
        if(c[0]<1||c[0]>St7.kMaxElementNode)throw new InvalidDataException("Invalid connectivity length");
        return (property,c.Skip(1).Take(c[0]).Select(n=>nodes[n]).ToArray());
    }
    private void Identity(string kind,int number,int label,int modelId) => data.Identities.Add(new(kind,number,label,modelId));
    public ModelImportData Read()
    {
        int freedoms=0;Check(St7.St7GetNumFreedomCase(id,ref freedoms),"Read freedom cases");
        if(freedom>freedoms)throw new ArgumentOutOfRangeException(nameof(freedom),"Freedom case is outside this file");
        if(freedoms>1)data.Issues.Add(new("Freedom cases",$"Imported case {freedom} of {freedoms}; select another case to create another Model.",false));
        int stages=0;Check(St7.St7GetNumStages(id,ref stages),"Read stages");
        if(stages>1)Omit("Stages","Staged construction is not represented in Model.");
        byte nonlinearMaterial=0;Check(St7.St7GetSolverNonlinearMaterial(id,ref nonlinearMaterial),"Read material analysis option");
        if(nonlinearMaterial!=0)Omit("Materials","Nonlinear material laws are not imported; only elastic constants can be represented.");
        ReadCases();
        for(int n=1,count=Total(St7.tyNODE);n<=count;n++)
        {
            var xyz=Values(3,v=>St7.St7GetNodeXYZ(id,n,v),"Read node");int label=0;Check(St7.St7GetNodeID(id,n,ref label),"Read node label");
            var node=new NodeElementModel(xyz[0],xyz[1],xyz[2]){Id=n};
            nodes[n]=node;data.Nodes.Add(node);Identity("Node",n,label,n);ReadNodeAttributes(n,node);
        }
        for(int n=1,count=Total(St7.tyBEAM);n<=count;n++)
        {
            string owner=$"Beam {n}";
            try
            {
                var c=Connection(St7.tyBEAM,n);int label=0;Check(St7.St7GetBeamID(id,n,ref label),"Read beam label");
                if(c.Nodes.Length!=2)throw new NotSupportedException("Only two-node beam connectivity is supported.");
                int type=0;Check(St7.St7GetBeamPropertyType(id,c.Property,ref type),"Read beam type");
                if(type==St7.btConnection){ReadConnection(n,c.Property,c.Nodes,label);continue;}
                var beam=new FrameElementModel(c.Nodes[0],c.Nodes[1],BeamProperty(c.Property),0,Vector3d.Zero){Id=n,Groups=Group(St7.tyBEAM,n)};
                var axes=Values(9,v=>St7.St7GetBeamAxisSystemInitial(id,n,v),"Read beam axes");beam=ElementAlignment.Beam(beam,Vector(axes));
                ReadBeamAttributes(n,beam);data.Beams.Add(beam);Identity("Beam",n,label,n);
            }
            catch(NotSupportedException ex){Omit(owner,ex.Message);}
        }
        for(int n=1,count=Total(St7.tyPLATE);n<=count;n++)
        {
            try
            {
                var c=Connection(St7.tyPLATE,n);if(c.Nodes.Length is not (3 or 4))throw new NotSupportedException("Only Tri3 and Quad4 are supported; no automatic linearisation.");
                var plate=new AreaElementModel(c.Nodes.ToList(),PlateProperty(c.Property)){Id=n,Groups=Group(St7.tyPLATE,n)};
                var axes=Values(9,v=>St7.St7GetPlateAxisSystemInitial(id,n,v),"Read plate axes");plate=ElementAlignment.Plate(plate,Vector(axes));
                ReadPlateAttributes(n,plate);data.Plates.Add(plate);int label=0;Check(St7.St7GetPlateID(id,n,ref label),"Read plate label");Identity("Plate",n,label,n);
            }
            catch(NotSupportedException ex){Omit($"Plate {n}",ex.Message);}
        }
        for(int n=1,count=Total(St7.tyBRICK);n<=count;n++)
        {
            try
            {
                var c=Connection(St7.tyBRICK,n);if(c.Nodes.Length is not (4 or 6 or 8))throw new NotSupportedException("Only Tet4, Wedge6 and Hexa8 are supported.");
                var solid=new SolidElementModel{Id=n,Nodes=c.Nodes.ToList(),Material=SolidMaterial(c.Property),Groups=Group(St7.tyBRICK,n)};
                ReadSolidAttributes(n,solid);data.Solids.Add(solid);int label=0;Check(St7.St7GetBrickID(id,n,ref label),"Read solid label");Identity("Solid",n,label,n);
            }
            catch(NotSupportedException ex){Omit($"Solid {n}",ex.Message);}
        }
        ReadLinks();ReadCombinations();
        data.Issues.Add(new("Scope","Elastic structural data, selected freedom case and active linear combinations only. Sections retain numerical stiffness and optional preview contours, not the original library definition or stress-recovery discretisation. Solver settings, material constitutive laws, CAD geometry, design metadata and result files are outside this scope.",false));
        return data;
    }
}
