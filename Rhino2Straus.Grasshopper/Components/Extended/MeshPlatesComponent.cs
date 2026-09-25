using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Helper;
using Rhino2Straus.Core.Loads;
using Rhino2Straus.Core.Models;
using Rhino2Straus.Core.Settings;
using Rhino2Straus.Grasshopper.Datatype;
using Rhino2Straus.Import;
using St7API;

namespace Rhino2Straus.Grasshopper.Components.Extended;

public sealed class MeshPlatesComponent : SafeComponent
{
    public MeshPlatesComponent() : base("Mesh to Plates", "Mesh to Plates", "Mesh to Plates", "Rhino2Straus", "03-Elements") { }
    public override Guid ComponentGuid => new Guid("24b5c060-c59b-536b-bad7-5ea1bba7a24e");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
        p.AddGenericParameter("Property", "Property", "Property", GH_ParamAccess.item);
        p.AddIntegerParameter("First ID", "First ID", "Positive first ID; -1 lets Build Model assign IDs", GH_ParamAccess.item, -1);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddGenericParameter("Plates", "Plates", "Plates", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var mesh=Item<Mesh>(da,0); var property=Item<GH_AreaProperty>(da,1).Value; int first=Item<int>(da,2);
        if(!mesh.IsValid || mesh.Faces.Count==0 || first==0 || first < -1) throw new ArgumentException("Use a valid mesh and a positive first ID, or -1.");
        var plates=new List<GH_AreaElement>();
        for(int i=0;i<mesh.Faces.Count;i++) { var f=mesh.Faces[i]; var indices=f.IsTriangle?new[]{f.A,f.B,f.C}:new[]{f.A,f.B,f.C,f.D};
            var nodes=indices.Select(j=>new NodeElementModel(mesh.Vertices[j])).ToList();
            plates.Add(new GH_AreaElement(new AreaElementModel(nodes,property){Id=first<0?-1:checked(first+i)})); }
        da.SetDataList(0,plates);
    }
}
