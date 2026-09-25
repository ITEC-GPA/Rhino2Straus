using System;
using Rhino2Straus.Core.Settings;
using St7API;

namespace Rhino2Straus.Core.Helper;

public static class StrausUnits
{
    public static double ThermalExpansion(double perCelsius, ModelUnitsModel units) => units.TemperatureUnit==ModelUnitsModel.TemperatureUnitTypes.F?perCelsius/1.8:perCelsius;
    // Material stress is always kPa and mass is always tonne. Geometry follows LengthUnit.
    public static int[] ToApi(ModelUnitsModel units)
    {
        if (units == null) throw new ArgumentNullException(nameof(units));
        int[] result = new int[St7.kLastUnit];
        result[St7.ipLENGTHU] = units.LengthUnit switch
        {
            ModelUnitsModel.LengthUnitTypes.M => St7.luMETRE,
            ModelUnitsModel.LengthUnitTypes.CM => St7.luCENTIMETRE,
            ModelUnitsModel.LengthUnitTypes.MM => St7.luMILLIMETRE,
            _ => throw new ArgumentOutOfRangeException(nameof(units.LengthUnit))
        };
        result[St7.ipFORCEU] = units.ForceUnit switch
        {
            ModelUnitsModel.ForceUnitTypes.KN => St7.fuKILONEWTON,
            ModelUnitsModel.ForceUnitTypes.N => St7.fuNEWTON,
            _ => throw new ArgumentOutOfRangeException(nameof(units.ForceUnit))
        };
        result[St7.ipTEMPERU] = units.TemperatureUnit switch
        {
            ModelUnitsModel.TemperatureUnitTypes.C => St7.tuCELSIUS,
            ModelUnitsModel.TemperatureUnitTypes.F => St7.tuFAHRENHEIT,
            _ => throw new ArgumentOutOfRangeException(nameof(units.TemperatureUnit))
        };
        result[St7.ipENERGYU] = units.HeatUnit switch
        {
            ModelUnitsModel.HeatUnitTypes.J => St7.euJOULE,
            ModelUnitsModel.HeatUnitTypes.KJ => St7.euKILOJOULE,
            ModelUnitsModel.HeatUnitTypes.CAL => St7.euCALORIE,
            ModelUnitsModel.HeatUnitTypes.KCAL => throw new NotSupportedException("Straus7 R3 has no kilocalorie unit; select joule, kilojoule, calorie or BTU."),
            ModelUnitsModel.HeatUnitTypes.BTU => St7.euBTU,
            _ => throw new ArgumentOutOfRangeException(nameof(units.HeatUnit))
        };
        result[St7.ipSTRESSU] = St7.suKILOPASCAL;
        result[St7.ipMASSU] = St7.muTONNE;
        return result;
    }

    public static double LengthPerMetre(ModelUnitsModel.LengthUnitTypes unit) => unit switch
    {
        ModelUnitsModel.LengthUnitTypes.M => 1,
        ModelUnitsModel.LengthUnitTypes.CM => 100,
        ModelUnitsModel.LengthUnitTypes.MM => 1000,
        _ => throw new ArgumentOutOfRangeException(nameof(unit))
    };
}
