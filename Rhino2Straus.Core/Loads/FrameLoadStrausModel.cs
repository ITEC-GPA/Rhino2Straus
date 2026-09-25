using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;
using System.ComponentModel;

namespace Rhino2Straus.Core.Loads
{
    public class FrameLoadStrausModel : FrameLoadBaseModel
    {
        public enum FrameLoadTypes
        {
            Force,
            Moment,
        }

        public enum LoadDirections
        {
            X,
            Y,
            Z,
            X_Projected,
            Y_Projected,
            Z_Projected,
            Gravity,
            Gravity_Projected
        }

        public enum LoadSchemas
        {
            Undefined = -1,
            Uniform,
            Keystone,
            Triangular,
            [Description("Triangular at end 1 + Keystone")]
            TriangularEnd1_Keystone,
            [Description("Keystone + Triangular at end 2")]
            Keystone_TriangularEnd2,
            [Description("Triangular at ends + Keystone")]
            TriangularEnds_Keystone
        }

        public FrameLoadTypes LoadType { get; set; }

        public LoadDirections Direction { get; set; }

        public LoadSchemas LoadSchema { get; set; }

        public bool IsProjected { get; set; }

        public double A { get; set; }

        public double PA { get; set; }

        public double B { get; set; }

        public double PB { get; set; }

        public double P1 { get; set; }

        public double P2 { get; set; }

        public FrameLoadStrausModel(LoadCaseModel loadCase, FrameLoadTypes forceOrMoment, LoadDirections direction, bool isProjected, double startLocationRelative, double startLoad, double endLocationRelative, double endLoad, LoadSchemas loadSchema, CoordinateSystemModel coordinateSystemModel)
        {
            LoadCase = loadCase;
            LoadType = forceOrMoment;
            Direction = direction;
            IsProjected = isProjected;
            A = startLocationRelative;
            PA = startLoad;
            B = endLocationRelative;
            PB = endLoad;
            LoadSchema = loadSchema;
            CoordinateSystem = coordinateSystemModel;
        }

        public FrameLoadStrausModel()
        {
        }

        public FrameLoadStrausModel(FrameLoadStrausModel frameLoadModel)
            :base(frameLoadModel)
        {
            LoadCase = frameLoadModel.LoadCase;
            LoadType = frameLoadModel.LoadType;
            Direction = frameLoadModel.Direction;
            IsProjected = frameLoadModel.IsProjected;
            A = frameLoadModel.A;
            PA = frameLoadModel.PA;
            P1 = frameLoadModel.P1;
            P2 = frameLoadModel.P2;
            B= frameLoadModel.B;
            PB = frameLoadModel.PB;
            LoadSchema = frameLoadModel.LoadSchema;
        }
    }
}
