namespace Rhino2Straus.Core.Settings
{
    public class ModelUnitsModel
    {
        public enum ForceUnitTypes
        {
            KN,
            N,
        }

        public enum LengthUnitTypes
        {
            M,
            CM,
            MM,
        }

        public enum HeatUnitTypes
        {
            KJ,
            J,
            BTU,
            KCAL,
            CAL,
        }

        public enum TemperatureUnitTypes
        {
            C,
            F,
        }

        public ForceUnitTypes ForceUnit { get; set; }

        public LengthUnitTypes LengthUnit { get; set; }

        public HeatUnitTypes HeatUnit { get; set; }

        public TemperatureUnitTypes TemperatureUnit { get; set; }

        public double Tolerance { get; set; } = 0.001;

        public ModelUnitsModel(ForceUnitTypes forceUnit, LengthUnitTypes lengthUnit, HeatUnitTypes heatUnit, TemperatureUnitTypes temperatureUnit, double tolerance)
        {
            ForceUnit = forceUnit;
            LengthUnit = lengthUnit;
            HeatUnit = heatUnit;
            TemperatureUnit = temperatureUnit;
            Tolerance = tolerance;
        }

        public ModelUnitsModel(ModelUnitsModel source)
            : this(source.ForceUnit, source.LengthUnit, source.HeatUnit, source.TemperatureUnit, source.Tolerance) { }

        public ModelUnitsModel()
        {
        }
    }
}
