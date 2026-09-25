using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;

namespace Rhino2Straus.Core.Loads
{
    public class AreaPressureLoadModel : AreaLoadBaseModel
    {
        public double Value { get; set; }

        public AreaPressureLoadModel(LoadCaseModel loadCase, double p1 = 0)
            :base(loadCase)
        {
            Value = p1;
        }

        public AreaPressureLoadModel()
        {
        }

        public AreaPressureLoadModel(AreaPressureLoadModel areaLoadModel)
            : base(areaLoadModel)
        {
            Value = areaLoadModel.Value;
        }
    }
}
