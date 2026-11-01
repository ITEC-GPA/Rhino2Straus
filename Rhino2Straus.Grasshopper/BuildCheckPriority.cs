using Grasshopper.Kernel;

namespace Rhino2Straus.Grasshopper
{
    /// <summary>Starts the load-time check shared by the gPC plug-ins: a bin built from another commit than the checkout is reported.</summary>
    public sealed class BuildCheckPriority : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Gpc.PluginChecks.PluginBuildCheck.Start(typeof(BuildCheckPriority).Assembly, "Rhino2Straus");
            return GH_LoadingInstruction.Proceed;
        }
    }
}
