using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Rhino2Straus.Grasshopper
{
    public class Rhino2Straus_GrasshopperInfo : GH_AssemblyInfo
    {
        public override string Name => "Rhino2Straus";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => ComponentIcons.Get("Rhino2Straus", "Model");

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "Straus7 R3 modelling, export, analysis and results for Rhino 8 / Grasshopper";

        public override Guid Id => new Guid("d352eafe-8ed1-450f-98c9-10c0185b8317");

        //Return a string identifying you or your company.
        public override string AuthorName => "Gabriele Pacini";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "gabrielepacini6293@gmail.com";

        //Return a string representing the version.  This returns the same version as the assembly.
        public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
    }
}
