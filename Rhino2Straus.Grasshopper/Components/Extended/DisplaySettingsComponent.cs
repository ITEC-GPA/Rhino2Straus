using System;
using System.Linq;
using Grasshopper.Kernel;
using Rhino2Straus.Core.Settings;
namespace Rhino2Straus.Grasshopper.Components.Extended;

public sealed class DisplaySettingsComponent : SafeComponent
{
    private static readonly PreviewLayer[] Layers=Enum.GetValues<PreviewLayer>().Where(v=>v is not PreviewLayer.None and not PreviewLayer.All).ToArray();
    public DisplaySettingsComponent():base("Rhino2Straus Display Settings","Display Settings","Independent preview categories, scales, transparency and shading. Display only; does not alter Model data.","Rhino2Straus","13-Preview") { }
    public override Guid ComponentGuid=>new("aa5719cc-51c8-55ba-b181-86e95c723238");
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddBooleanParameter("Enabled","On","Master visibility",GH_ParamAccess.item,true);
        foreach(var layer in Layers)p.AddBooleanParameter(layer.ToString(),layer.ToString(),layer==PreviewLayer.Values?"Values and units for load/attribute symbols":"Show "+layer,GH_ParamAccess.item,new PreviewSettings().Shows(layer));
        p.AddBooleanParameter("Shaded","Shaded","Shaded surfaces; false keeps visible wires",GH_ParamAccess.item,true);
        p.AddNumberParameter("Symbol scale","Scale","Symbol size and axis length in model units",GH_ParamAccess.item,1);
        p.AddNumberParameter("Load scale","Loads","Load arrow multiplier, capped at six symbol lengths",GH_ParamAccess.item,1);
        p.AddNumberParameter("Transparency","Alpha","0=opaque, 1=transparent",GH_ParamAccess.item,.3);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Settings","Settings","Connect to Rhino2Straus Display Manager",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)
    {
        var mask=PreviewLayer.None;int i=1;foreach(var layer in Layers)if(Item<bool>(da,i++))mask|=layer;
        var options=new PreviewSettings{Enabled=Item<bool>(da,0),Layers=mask,Shaded=Item<bool>(da,i++),SymbolScale=Item<double>(da,i++),LoadScale=Item<double>(da,i++),Transparency=Item<double>(da,i)};
        options.Validate();da.SetData(0,options);
    }
}
