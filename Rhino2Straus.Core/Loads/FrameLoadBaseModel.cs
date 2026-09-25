using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;

namespace Rhino2Straus.Core.Loads
{
    public abstract class FrameLoadBaseModel
    {
        public FrameLoadBaseModel Duplicate()
        {
            var copy=(FrameLoadBaseModel)MemberwiseClone();copy.LoadCase=LoadCase==null?null:new LoadCaseModel(LoadCase);return copy;
        }
        public LoadCaseModel LoadCase { get; set; }

        public CoordinateSystemModel CoordinateSystem { get; set; }

        public FrameLoadBaseModel(LoadCaseModel loadCase, CoordinateSystemModel coordinateSystem)
        {
            LoadCase = loadCase;
            CoordinateSystem = coordinateSystem;
        }

        public FrameLoadBaseModel()
        {
        }

        public FrameLoadBaseModel(FrameLoadBaseModel frameLoadModel)
        {
            LoadCase = frameLoadModel.LoadCase;
            CoordinateSystem = frameLoadModel.CoordinateSystem;
        }
    }
}
