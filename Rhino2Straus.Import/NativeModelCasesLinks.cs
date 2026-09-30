using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.ElementProperties;
using St7API;

namespace Rhino2Straus.Import;

internal sealed partial class NativeModelReader
{
    private void ReadCases()
    {
        int count=0;Check(St7.St7GetNumLoadCase(id,ref count),"Read case count");var names=new HashSet<string>();
        for(int n=1;n<=count;n++)
        {
            var text=new StringBuilder(1024);Check(St7.St7GetLoadCaseName(id,n,text,text.Capacity),"Read case name");
            string name=text.ToString().Trim();if(name.Length==0)name=$"Case {n}";while(!names.Add(name))name+=$" [{n}]";
            var lc=new LoadCaseModel(name){Id=n};cases[n]=lc;data.Cases.Add(lc);Identity("Load case",n,n,n);
            int type=0;Check(St7.St7GetLoadCaseType(id,n,ref type),"Read inertia type");
            if(type!=St7.lcSeismic)
            {
                var defaults=Values(13,v=>St7.St7GetLoadCaseDefaults(id,n,v),"Read case defaults");
                lc.ReferenceTemperature=defaults[St7.ipLoadCaseRefTemp];
            }
            byte structural=0,nonstructural=0;Check(St7.St7GetLoadCaseMassOption(id,n,ref structural,ref nonstructural),"Read gravity mass options");
            lc.GravityNoStructuralMass=nonstructural!=0;
            if(type==St7.lcGravity)
            {
                int direction=0;double value=0;Check(St7.St7GetLoadCaseGravityDir(id,n,ref direction),"Read gravity direction");Check(St7.St7GetLoadCaseGravity(id,n,ref value),"Read gravity acceleration");
                if(data.SelfWeight!=null){Omit($"Load case {n}","More than one gravity case: Model currently supports one Self Weight.");continue;}
                data.SelfWeight=new SelfWeightModel{LoadCase=lc,GlobalInertiaLoad=SelfWeightModel.GlobalInertiaLoads.Gravity,
                    GravityDirection=(SelfWeightModel.GravityDirections)(direction-1),GravityValue=value,StructuralMassAcceleration=structural!=0,NonStructuralMassAcceleration=nonstructural!=0};
            }
            else if(type!=St7.lcNoInertia)Omit($"Load case {n}","Acceleration/velocity/seismic inertia is not represented in Model.");
        }
    }
    private void ReadCombinations()
    {
        var spectralFile=new StringBuilder(2048);Check(St7.St7GetLSACombinationSRAName(id,spectralFile,spectralFile.Capacity),"Read spectral combination reference");
        if(spectralFile.Length>0)Omit("Combinations","An SRA result file is referenced; spectral combination terms are not imported.");
        int count=0,freedoms=0;Check(St7.St7GetNumLSACombinations(id,ref count),"Read combination count");Check(St7.St7GetNumFreedomCase(id,ref freedoms),"Read freedom case count");
        var names=new HashSet<string>();
        for(int n=1;n<=count;n++)
        {
            var text=new StringBuilder(1024);Check(St7.St7GetLSACombinationName(id,n,text,text.Capacity),"Read combination name");
            string name=text.ToString().Trim();if(name.Length==0)name=$"Combination {n}";while(!names.Add(name))name+=$" [{n}]";
            byte enabled=0;Check(St7.St7GetLSACombinationState(id,n,ref enabled),"Read combination state");
            if(enabled==0){data.Issues.Add(new($"Combination {n}","Disabled in the file; omitted from the active Model.",false));continue;}
            var factors=new List<LoadFactorModel>();bool otherFreedom=false;
            foreach(var pair in cases)for(int fc=1;fc<=freedoms;fc++)
            {
                double value=0;Check(St7.St7GetLSACombinationFactor(id,St7.ltLoadCase,n,pair.Key,fc,ref value),"Read combination factor");
                if(value==0)continue;if(fc==freedom)factors.Add(new(pair.Value,value));else otherFreedom=true;
            }
            if(otherFreedom){Omit($"Combination {n}","References another freedom case; cannot combine different restraint sets in one Model.");continue;}
            if(factors.Count==0){Omit($"Combination {n}","No supported load factors in the selected freedom case.");continue;}
            data.Combinations.Add(new(name,factors,LoadCombinationModel.LoadCombinationTypes.Linear){Id=n});Identity("Combination",n,n,n);
        }
        int combined=0;Check(St7.St7GetNumCombinedLSACombinations(id,ref combined),"Read external result combinations");
        if(combined>0)Omit("Combinations","User-generated LSA result combinations are not structural load combinations.");
    }
    private void ReadConnection(int n,int property,NodeElementModel[] endpoints,int label)
    {
        for(int end=1;end<=2;end++){int ucs=0;Check(St7.St7GetBeamConnectionUCS(id,n,end,ref ucs),"Read connection axes");GlobalUcs(ucs);}
        var k=Values(6,v=>St7.St7GetConnectionData(id,property,v),"Read connection stiffness");
        var link=new LinkElementModel(endpoints[0],endpoints[1],new LinkPropertyModel(LinkPropertyModel.LinkPropertyTypes.GEN,k[0],k[1],k[2],k[3],k[4],k[5]))
            {Id=data.Links.Count+1,Groups=Group(St7.tyBEAM,n)};
        ReadAttributes(St7.tyBEAM,n,a=>{if(a.Kind!=St7.aoBeamConnectionUCS)Unsupported(a);});
        data.Links.Add(link);Identity("Connection beam",n,label,link.Id);
    }
    private void ReadLinks()
    {
        for(int n=1,count=Total(St7.tyLINK);n<=count;n++)
        {
            try
            {
                int type=0,ucs=1,plane=0,axis=0,label=0;var c=new int[3];var dofs=new int[6];
                Check(St7.St7GetLinkType(id,n,ref type),"Read link type");Check(St7.St7GetLinkID(id,n,ref label),"Read link label");
                LinkPropertyModel.LinkPropertyTypes kind;
                switch(type)
                {
                    case St7.ltRigidLink:Check(St7.St7GetRigidLink(id,n,ref ucs,ref plane,c),"Read rigid link");kind=LinkPropertyModel.LinkPropertyTypes.RIGID;break;
                    case St7.ltPinnedLink:Check(St7.St7GetPinnedLink(id,n,c),"Read pinned link");kind=LinkPropertyModel.LinkPropertyTypes.PINNED;break;
                    case St7.ltMasterSlaveLink:Check(St7.St7GetMasterSlaveLink(id,n,ref ucs,c,dofs),"Read master-slave link");kind=LinkPropertyModel.LinkPropertyTypes.MASTER_SLAVE;break;
                    case St7.ltSectorSymmetryLink:Check(St7.St7GetSectorSymmetryLink(id,n,ref axis,c),"Read sector symmetry link");kind=LinkPropertyModel.LinkPropertyTypes.SECTOR_SYMMETRY;break;
                    default:throw new NotSupportedException($"Link type {type} is not supported.");
                }
                GlobalUcs(ucs);if(c[0]!=2)throw new NotSupportedException("Only two-node links are supported.");
                var p=new LinkPropertyModel(kind);if(kind==LinkPropertyModel.LinkPropertyTypes.RIGID)p.Options.RigidPlane=plane;
                if(kind==LinkPropertyModel.LinkPropertyTypes.MASTER_SLAVE)p.Options.Dofs=dofs;
                if(kind==LinkPropertyModel.LinkPropertyTypes.SECTOR_SYMMETRY)p.Options.SymmetryAxis=axis;
                var link=new LinkElementModel(nodes[c[1]],nodes[c[2]],p){Id=data.Links.Count+1,Groups=Group(St7.tyLINK,n)};
                data.Links.Add(link);Identity("Link",n,label,link.Id);
            }
            catch(NotSupportedException ex){Omit($"Link {n}",ex.Message);}
        }
    }
}
