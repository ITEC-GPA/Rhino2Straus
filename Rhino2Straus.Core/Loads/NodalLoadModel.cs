using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;

namespace Rhino2Straus.Core.Loads
{
    public class NodalLoadModel
    {
        public LoadCaseModel LoadCase { get; set; }

        public double FX { get; set; }

        public double FY { get; set; }

        public double FZ { get; set; }

        public double MX { get; set; }

        public double MY { get; set; }

        public double MZ { get; set; }

        public LoadGroupModel LoadGroup { get; set; }

        public NodalLoadModel(LoadCaseModel loadCase, double fX, double fY, double fZ, double mX, double mY, double mZ)
        {
            LoadCase = loadCase;
            FX = fX;
            FY = fY;
            FZ = fZ;
            MX = mX;
            MY = mY;
            MZ = mZ;
        }

        public NodalLoadModel()
        {
        }

        public NodalLoadModel(NodalLoadModel nodalLoadModel)
        {
            LoadCase = nodalLoadModel.LoadCase;
            FX = nodalLoadModel.FX;
            FY = nodalLoadModel.FY;
            FZ = nodalLoadModel.FZ;
            MX = nodalLoadModel.MX;
            MY = nodalLoadModel.MY;
            MZ = nodalLoadModel.MZ;
            LoadGroup = nodalLoadModel.LoadGroup == null ? null : new LoadGroupModel(nodalLoadModel.LoadGroup);
        }
    }
}
