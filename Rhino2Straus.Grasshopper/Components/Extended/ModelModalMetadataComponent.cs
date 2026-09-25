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

public sealed class ModelModalMetadataComponent : SafeComponent
{
    public ModelModalMetadataComponent() : base("Model Modal and Buckling Data", "Model Modal and Buckling Data", "Model Modal and Buckling Data", "Rhino2Straus", "12-Results") { }
    public override Guid ComponentGuid => new Guid("73d59caf-4da7-5dbe-8303-8ce80638ee45");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddNumberParameter("Frequency Hz", "Frequency Hz", "One branch per result case, NFA only", GH_ParamAccess.tree);
        p.AddNumberParameter("Buckling factor", "Buckling factor", "One branch per result case, LBA only", GH_ParamAccess.tree);
        p.AddNumberParameter("Modal data", "Modal data", "NFA: frequency, mass, stiffness, damping, 6 mass participation ratios, 6 mode participation factors", GH_ParamAccess.tree);
        p.AddTextParameter("Modal columns", "Modal columns", "Modal columns", GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {

        var m=Item<GH_Model>(da,0).Value;var r=m.Results??throw new InvalidOperationException("Model has no results.");r.ValidateFor(m);
        var frequencies=new GH_Structure<GH_Number>();var factors=new GH_Structure<GH_Number>();var modal=new GH_Structure<GH_Number>();
        foreach(var c in r.Cases){if(c.FrequencyHz.HasValue)frequencies.Append(new GH_Number(c.FrequencyHz.Value),new GH_Path(c.Number));if(c.BucklingFactor.HasValue)factors.Append(new GH_Number(c.BucklingFactor.Value),new GH_Path(c.Number));}
        foreach(var mode in r.Modal)modal.AppendRange(mode.Values.Select(x=>new GH_Number(x)),new GH_Path(mode.CaseNumber));
        da.SetDataTree(0,frequencies);da.SetDataTree(1,factors);da.SetDataTree(2,modal);da.SetDataList(3,new[]{"FrequencyHz","ModalMass","ModalStiffness","ModalDamping","TMassP1","TMassP2","TMassP3","RMassP1","RMassP2","RMassP3","ModeD1","ModeD2","ModeD3","ModeR1","ModeR2","ModeR3"});
    }
}
