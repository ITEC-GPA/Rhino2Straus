using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;

namespace Rhino2Straus.Core.Loads
{
    public abstract class AreaLoadBaseModel
    {
        public AreaLoadBaseModel Duplicate()
        {
            var copy=(AreaLoadBaseModel)MemberwiseClone();copy.LoadCase=LoadCase==null?null:new LoadCaseModel(LoadCase);return copy;
        }
        public LoadCaseModel LoadCase { get; set; }
        public CoordinateSystemModel CoordinateSystem { get; set; }

        public AreaLoadBaseModel(LoadCaseModel loadCase, CoordinateSystemModel coordinateSystem = null)
        {
            LoadCase = loadCase;
            if(coordinateSystem != null)
                CoordinateSystem = coordinateSystem;
        }

        public AreaLoadBaseModel()
        {
        }

        public AreaLoadBaseModel(AreaLoadBaseModel areaLoadModel)
        {
            LoadCase = areaLoadModel.LoadCase;
            CoordinateSystem = areaLoadModel.CoordinateSystem;
        }
    }
}
