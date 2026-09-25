namespace Rhino2Straus.Core.Cases
{
    public class SelfWeightModel
    {
        private static double GRAVITY_mmSuS2 = -9806.65; // mm/s^2,
        public enum GlobalInertiaLoads
        {
            None,
            Gravity,
            AccelerationAndVelocity,
            Seismic
        }

        public enum GravityDirections
        {
            X,
            Y,
            Z
        };

        public LoadCaseModel LoadCase { get; set; }

        public double FactorX { get; set; }

        public double FactorY { get; set; }

        public double FactorZ { get; set; }

        public bool StructuralMassAcceleration { get; set; }

        public bool NonStructuralMassAcceleration { get; set; }

        public GlobalInertiaLoads GlobalInertiaLoad { get; set; }

        public GravityDirections GravityDirection { get; set; }

        public double GravityValue { get; set; }

        public SelfWeightModel(LoadCaseModel loadCase, double factorX, double factorY, double factorZ)
        {
            LoadCase = loadCase;
            FactorX = factorX;
            FactorY = factorY;
            FactorZ = factorZ;
            GlobalInertiaLoad = GlobalInertiaLoads.Gravity;
            GravityDirection = GravityDirections.Z;
            GravityValue = GRAVITY_mmSuS2;
            if (loadCase.GravityNoStructuralMass)
            {
                StructuralMassAcceleration = false;
                NonStructuralMassAcceleration = true;
            }
        }

        public SelfWeightModel()
        {
        }

        public SelfWeightModel(SelfWeightModel model)
        {
            GlobalInertiaLoad = model.GlobalInertiaLoad;
            GravityDirection = model.GravityDirection;
            GravityValue = model.GravityValue;
            StructuralMassAcceleration = model.StructuralMassAcceleration;
            NonStructuralMassAcceleration = model.NonStructuralMassAcceleration;
            FactorX = model.FactorX;
            FactorY = model.FactorY;
            FactorZ = model.FactorZ;
            LoadCase = new LoadCaseModel(model.LoadCase);
        }

        public static SelfWeightModel DeadLoad => new SelfWeightModel
        {
            LoadCase = new LoadCaseModel("Dead", LoadCaseModel.LoadCaseTypes.D, "Self Weight of Structure") { Id = 1 },
            GlobalInertiaLoad = GlobalInertiaLoads.Gravity,
            GravityDirection = GravityDirections.Z,
            GravityValue = GRAVITY_mmSuS2,
            StructuralMassAcceleration = true,
            NonStructuralMassAcceleration = false,
        };
    }
}
