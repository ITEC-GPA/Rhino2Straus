using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
namespace Rhino2Straus.Grasshopper.Datatype;

public interface IPreviewController { bool ManagesDocumentPreview { get; } bool AllowsResultPreview { get; } }

/// <summary>Document-scoped ownership. Removing a manager restores previews without changing Hidden flags.</summary>
public static class PreviewPolicy
{
    public static bool ComponentVisible(GH_Component component)
    {
        var document=component.OnPingDocument()??Instances.ActiveCanvas?.Document;
        if(document==null)return true;
        if(AutomaticVisible(document))return true;
        return component.SubCategory=="12-Results"&&document.Objects.OfType<IPreviewController>().Any(c=>c.ManagesDocumentPreview&&c.AllowsResultPreview);
    }
    public static bool AutomaticVisible(GH_Document document=null)
    {
        document??=Instances.ActiveCanvas?.Document;
        return document==null||!document.Objects.OfType<IPreviewController>().Any(c=>c.ManagesDocumentPreview);
    }
}
