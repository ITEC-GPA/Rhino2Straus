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

public sealed class DecomposeResultRecordComponent : SafeComponent
{
    public DecomposeResultRecordComponent() : base("Decompose Result Record", "Decompose Result Record", "Decompose Result Record", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("6f6e9a49-8187-5e36-a12e-2a5ac04ac815");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Record", "Record", "Record", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddIntegerParameter("Case", "Case", "Case", GH_ParamAccess.item);
        p.AddTextParameter("Entity type", "Entity type", "Entity type", GH_ParamAccess.item);
        p.AddIntegerParameter("Entity ID", "Entity ID", "Entity ID", GH_ParamAccess.item);
        p.AddIntegerParameter("Native number", "Native number", "Native number", GH_ParamAccess.item);
        p.AddTextParameter("Quantity", "Quantity", "Quantity", GH_ParamAccess.item);
        p.AddIntegerParameter("Surface", "Surface", "Surface", GH_ParamAccess.item);
        p.AddTextParameter("Axes", "Axes", "Axes", GH_ParamAccess.item);
        p.AddTextParameter("Sampling", "Sampling", "Sampling", GH_ParamAccess.item);
        p.AddTextParameter("Columns", "Columns", "Columns", GH_ParamAccess.list);
        p.AddNumberParameter("Positions", "Positions", "Positions", GH_ParamAccess.list);
        p.AddNumberParameter("Values", "Values", "One branch per sample", GH_ParamAccess.tree);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var r=Item<GH_ObjectWrapper>(da,0).Value as ElementResult??throw new ArgumentException("Supply an ElementResult record.");
        da.SetData(0,r.CaseNumber);da.SetData(1,r.Entity.ToString());da.SetData(2,r.EntityId);da.SetData(3,r.NativeNumber);da.SetData(4,r.Quantity.ToString());da.SetData(5,r.Surface);da.SetData(6,r.Axes);da.SetData(7,r.Sampling);da.SetDataList(8,r.Columns);da.SetDataList(9,r.Positions);
        var tree=new GH_Structure<GH_Number>();for(int row=0;row<r.Values.Count;row++)tree.AppendRange(r.Values[row].Select(x=>new GH_Number(x)),new GH_Path(row));da.SetDataTree(10,tree);
    }
}
