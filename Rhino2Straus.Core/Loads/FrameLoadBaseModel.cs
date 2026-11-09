using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;

namespace Rhino2Straus.Core.Loads
{
    public abstract class FrameLoadBaseModel
    {
        public FrameLoadBaseModel Duplicate()
        {
            var copy = (FrameLoadBaseModel)MemberwiseClone();
            copy.LoadCase = LoadCase == null ? null : new LoadCaseModel(LoadCase);
            copy.CoordinateSystem = CoordinateSystem == null ? null : new CoordinateSystemModel(CoordinateSystem);
            return copy;
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
            CoordinateSystem = frameLoadModel.CoordinateSystem == null ? null : new CoordinateSystemModel(frameLoadModel.CoordinateSystem);
        }
    }
}
