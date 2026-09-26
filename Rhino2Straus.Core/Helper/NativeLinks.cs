using System;
using System.IO;
using System.Linq;
using Rhino2Straus.Api;
using Rhino2Straus.Core.ElementProperties;
using St7API;

namespace Rhino2Straus.Core.Helper;

public static class NativeLinks
{
    public static int NativeType(LinkPropertyModel.LinkPropertyTypes type) => type switch
    {
        LinkPropertyModel.LinkPropertyTypes.RIGID=>St7.ltRigidLink,
        LinkPropertyModel.LinkPropertyTypes.PINNED=>St7.ltPinnedLink,
        LinkPropertyModel.LinkPropertyTypes.MASTER_SLAVE=>St7.ltMasterSlaveLink,
        LinkPropertyModel.LinkPropertyTypes.SECTOR_SYMMETRY=>St7.ltSectorSymmetryLink,
        _=>throw new ArgumentException("This property is not a supported native link.")
    };
    public static void Write(int id,int number,LinkPropertyModel property,int start,int end)
    {
        property.Options.Validate();
        var c=new[]{2,start,end}; var o=property.Options;
        int code=property.Type switch
        {
            LinkPropertyModel.LinkPropertyTypes.RIGID=>St7.St7SetRigidLink(id,number,1,o.RigidPlane,c),
            LinkPropertyModel.LinkPropertyTypes.PINNED=>St7.St7SetPinnedLink(id,number,c),
            LinkPropertyModel.LinkPropertyTypes.MASTER_SLAVE=>St7.St7SetMasterSlaveLink(id,number,1,c,o.Dofs),
            LinkPropertyModel.LinkPropertyTypes.SECTOR_SYMMETRY=>St7.St7SetSectorSymmetryLink(id,number,o.SymmetryAxis,c),
            _=>throw new ArgumentException("Unsupported native link property.")
        };
        StrausRuntime.Check(code,"Write native link");
    }
    public static void ValidateFile(int id,int number,LinkPropertyModel property,int start,int end)
    {
        void Check(int code)=>StrausRuntime.Check(code,"Read native link");
        int type=0; Check(St7.St7GetLinkType(id,number,ref type));
        if(type!=NativeType(property.Type)) throw new InvalidDataException("Native link type differs from Model.");
        int ucs=1,plane=0,axis=0;var c=new int[3];var dofs=new int[6];bool same;
        switch(property.Type)
        {
            case LinkPropertyModel.LinkPropertyTypes.RIGID:
                Check(St7.St7GetRigidLink(id,number,ref ucs,ref plane,c)); same=plane==property.Options.RigidPlane; break;
            case LinkPropertyModel.LinkPropertyTypes.PINNED:
                Check(St7.St7GetPinnedLink(id,number,c)); same=true; break;
            case LinkPropertyModel.LinkPropertyTypes.MASTER_SLAVE:
                Check(St7.St7GetMasterSlaveLink(id,number,ref ucs,c,dofs)); same=dofs.SequenceEqual(property.Options.Dofs); break;
            case LinkPropertyModel.LinkPropertyTypes.SECTOR_SYMMETRY:
                Check(St7.St7GetSectorSymmetryLink(id,number,ref axis,c)); same=axis==property.Options.SymmetryAxis; break;
            default: throw new InvalidDataException("Unsupported native link.");
        }
        if(!same || ucs!=1 || !c.SequenceEqual(new[]{2,start,end})) throw new InvalidDataException("Native link connectivity or options differ from Model.");
    }
}
