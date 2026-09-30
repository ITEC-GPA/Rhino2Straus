using System;
using System.Collections.Generic;
using System.ComponentModel;
using Rhino2Straus.Core.Base;

namespace Rhino2Straus.Core.ElementProperties
{
    public class MaterialModel : ModelObjectId, IEquatable<MaterialModel>
    {
        public static MaterialModel C12_15 => new MaterialModel() { Type = MaterialTypes.Concrete, Name = "C12/15", DampingRatio = 0.05, Density = 2.549, Id = UNASSIGNED, ModulusElasticity = 2.7085e+07, PoissonRatio = 0.2, ThermalCoefficient = 1.0E-05, Standard = Standards.EN04_RC, CompressiveStrength = 12 };
        public static MaterialModel C16_20 => new MaterialModel() { Type = MaterialTypes.Concrete, Name = "C16/20", DampingRatio = 0.05, Density = 2.549, Id = UNASSIGNED, ModulusElasticity = 2.8607e+07, PoissonRatio = 0.2, ThermalCoefficient = 1.0E-05, Standard = Standards.EN04_RC, CompressiveStrength = 16 };
        public static MaterialModel C20_25 => new MaterialModel() { Type = MaterialTypes.Concrete, Name = "C20/25", DampingRatio = 0.05, Density = 2.549, Id = UNASSIGNED, ModulusElasticity = 2.9961e+07, PoissonRatio = 0.2, ThermalCoefficient = 1.0E-05, Standard = Standards.EN04_RC, CompressiveStrength = 20 };
        public static MaterialModel C25_30 => new MaterialModel() { Type = MaterialTypes.Concrete, Name = "C25/30", DampingRatio = 0.05, Density = 2.549, Id = UNASSIGNED, ModulusElasticity = 3.1475e+07, PoissonRatio = 0.2, ThermalCoefficient = 1.0E-05, Standard = Standards.EN04_RC, CompressiveStrength = 25 };
        public static MaterialModel C30_37 => new MaterialModel() { Type = MaterialTypes.Concrete, Name = "C30/37", DampingRatio = 0.05, Density = 2.549, Id = UNASSIGNED, ModulusElasticity = 3.2836e+07, PoissonRatio = 0.2, ThermalCoefficient = 1.0E-05, Standard = Standards.EN04_RC, CompressiveStrength = 30 };
        public static MaterialModel C35_45 => new MaterialModel() { Type = MaterialTypes.Concrete, Name = "C35/45", DampingRatio = 0.05, Density = 2.549, Id = UNASSIGNED, ModulusElasticity = 3.4077e+07, PoissonRatio = 0.2, ThermalCoefficient = 1.0E-05, Standard = Standards.EN04_RC, CompressiveStrength = 35 };
        public static MaterialModel C40_50 => new MaterialModel() { Type = MaterialTypes.Concrete, Name = "C40/50", DampingRatio = 0.05, Density = 2.549, Id = UNASSIGNED, ModulusElasticity = 3.5220e+07, PoissonRatio = 0.2, ThermalCoefficient = 1.0E-05, Standard = Standards.EN04_RC, CompressiveStrength = 40 };
        public static MaterialModel C45_55 => new MaterialModel() { Type = MaterialTypes.Concrete, Name = "C45/55", DampingRatio = 0.05, Density = 2.549, Id = UNASSIGNED, ModulusElasticity = 3.6283e+07, PoissonRatio = 0.2, ThermalCoefficient = 1.0E-05, Standard = Standards.EN04_RC, CompressiveStrength = 45 };
        public static MaterialModel C50_60 => new MaterialModel() { Type = MaterialTypes.Concrete, Name = "C50/60", DampingRatio = 0.05, Density = 2.549, Id = UNASSIGNED, ModulusElasticity = 3.7277e+07, PoissonRatio = 0.2, ThermalCoefficient = 1.0E-05, Standard = Standards.EN04_RC, CompressiveStrength = 50 };
        public static MaterialModel C55_67 => new MaterialModel() { Type = MaterialTypes.Concrete, Name = "C55/67", DampingRatio = 0.05, Density = 2.549, Id = UNASSIGNED, ModulusElasticity = 3.8214e+07, PoissonRatio = 0.2, ThermalCoefficient = 1.0E-05, Standard = Standards.EN04_RC, CompressiveStrength = 55 };
        public static MaterialModel C60_75 => new MaterialModel() { Type = MaterialTypes.Concrete, Name = "C60/75", DampingRatio = 0.05, Density = 2.549, Id = UNASSIGNED, ModulusElasticity = 3.9099e+07, PoissonRatio = 0.2, ThermalCoefficient = 1.0E-05, Standard = Standards.EN04_RC, CompressiveStrength = 60 };
        public static MaterialModel C70_85 => new MaterialModel() { Type = MaterialTypes.Concrete, Name = "C70/85", DampingRatio = 0.05, Density = 2.549, Id = UNASSIGNED, ModulusElasticity = 4.0742e+07, PoissonRatio = 0.2, ThermalCoefficient = 1.0E-05, Standard = Standards.EN04_RC, CompressiveStrength = 70 };
        public static MaterialModel C80_95 => new MaterialModel() { Type = MaterialTypes.Concrete, Name = "C80/95", DampingRatio = 0.05, Density = 2.549, Id = UNASSIGNED, ModulusElasticity = 4.2244e+07, PoissonRatio = 0.2, ThermalCoefficient = 1.0E-05, Standard = Standards.EN04_RC, CompressiveStrength = 80 };
        public static MaterialModel C90_105 => new MaterialModel() { Type = MaterialTypes.Concrete, Name ="C90/105", DampingRatio = 0.05, Density = 2.549, Id = UNASSIGNED, ModulusElasticity = 4.363e+07, PoissonRatio = 0.2, ThermalCoefficient = 1.0E-05, Standard = Standards.EN04_RC, CompressiveStrength = 90 };

        public static MaterialModel S235 => new MaterialModel() { Type = MaterialTypes.Steel, Name = "S235", DampingRatio = 0.02, Density = 7.849, Id = UNASSIGNED, ModulusElasticity = 2.1e+08, PoissonRatio = 0.3, ThermalCoefficient = 1.170E-05, Standard = Standards.EN05_S, TensileStrength = 360, TensileYeld = 235 };
        public static MaterialModel S275 => new MaterialModel() { Type = MaterialTypes.Steel, Name = "S275", DampingRatio = 0.02, Density = 7.849, Id = UNASSIGNED, ModulusElasticity = 2.1e+08, PoissonRatio = 0.3, ThermalCoefficient = 1.170E-05, Standard = Standards.EN05_S, TensileStrength = 430, TensileYeld = 275 };
        public static MaterialModel S355 => new MaterialModel() { Type = MaterialTypes.Steel, Name = "S355", DampingRatio = 0.02, Density = 7.849, Id = UNASSIGNED, ModulusElasticity = 2.1e+08, PoissonRatio = 0.3, ThermalCoefficient = 1.170E-05, Standard = Standards.EN05_S, TensileStrength = 510, TensileYeld = 355 };
        public static MaterialModel S450 => new MaterialModel() { Type = MaterialTypes.Steel, Name = "S450", DampingRatio = 0.02, Density = 7.849, Id = UNASSIGNED, ModulusElasticity = 2.1e+08, PoissonRatio = 0.3, ThermalCoefficient = 1.170E-05, Standard = Standards.EN05_S, TensileStrength = 550, TensileYeld = 450 };


        public static Dictionary<string, MaterialModel> ConcreteDatabase => new Dictionary<string, MaterialModel>()
        {
            { "C12/15", C12_15 },
            { "C16/20", C16_20 },
            { "C20/25", C20_25 },
            { "C25/30", C25_30 },
            { "C30/37", C30_37 },
            { "C35/45", C35_45 },
            { "C40/50", C40_50 },
            { "C45/55", C45_55 },
            { "C50/60", C50_60 },
            { "C55/67", C55_67 },
            { "C60/75", C60_75 } ,
            { "C70/85", C70_85 },
            { "C80/95", C80_95 },
            { "C90/105", C90_105 },
        };

        public static Dictionary<string, MaterialModel> SteelDatabase => new Dictionary<string, MaterialModel>()
        {
            { "S235", S235 },
            { "S275", S275 },
            { "S355", S355 },
            { "S450", S450 },
        };

        public enum Standards
        {
            Custom,
            [Description("EN05(S)")]
            EN05_S,
            [Description("EN04(RC)")]
            EN04_RC,
        }

        public enum MaterialTypes
        {
            [Description("STEEL")]
            Steel,
            [Description("CONC")]
            Concrete,
            [Description("ISOTROPIC")]
            Isotropic,
        }

        public MaterialTypes Type { get; set; }
        public Standards Standard { get; set; }

        public double DampingRatio { get; set; }
        public double ModulusElasticity { get; set; }
        public double PoissonRatio { get; set; }
        public double ShearModulus => ModulusElasticity / (2 * (1 + PoissonRatio));
        public double ThermalCoefficient { get; set; }
        public double Conductivity { get; set; }
        public double SpecificHeat { get; set; }
        public double ViscousDamping { get; set; }
        public double Density { get; set; }
        // Compatibility alias: Density already expresses mass per volume in tonne/m³.
        public double Mass => Density;

        /// <summary>Nominal compressive strength in MPa; zero means not defined.</summary>
        public double CompressiveStrength { get; set; }
        /// <summary>Nominal tensile strength in MPa; zero means not defined.</summary>
        public double TensileStrength { get; set; }
        /// <summary>Nominal yield strength in MPa; zero means not defined. Legacy spelling retained.</summary>
        public double TensileYeld { get; set; }
        public double YieldStressKPa => TensileYeld * 1000;
        public double UltimateStressKPa => TensileStrength * 1000;
        public double CompressiveStressKPa => CompressiveStrength * 1000;

        public MaterialModel(string name, MaterialTypes type, double dampingRatio, double modulusElasticity, double poissonRatio, double thermalCoefficient, double density, Standards standard)
            : base(name)
        {
            Type = type;
            DampingRatio = dampingRatio;
            ModulusElasticity = modulusElasticity;
            PoissonRatio = poissonRatio;
            ThermalCoefficient = thermalCoefficient;
            Density = density;
            Standard = standard;
        }

        public MaterialModel()
            : base()
        {
        }

        public MaterialModel(MaterialModel materialModel)
            : base(materialModel.Id, materialModel.Name)
        {
            Conductivity = materialModel.Conductivity; SpecificHeat = materialModel.SpecificHeat;
            ViscousDamping = materialModel.ViscousDamping; CompressiveStrength = materialModel.CompressiveStrength;
            TensileStrength = materialModel.TensileStrength; TensileYeld = materialModel.TensileYeld;
            Type = materialModel.Type;
            DampingRatio = materialModel.DampingRatio;
            ModulusElasticity = materialModel.ModulusElasticity;
            PoissonRatio = materialModel.PoissonRatio;
            ThermalCoefficient = materialModel.ThermalCoefficient;
            Density = materialModel.Density;
            Standard = materialModel.Standard;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MaterialModel);
        }

        public bool Equals(MaterialModel other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   Name == other.Name &&
                   Type == other.Type &&
                   DampingRatio == other.DampingRatio &&
                   ModulusElasticity == other.ModulusElasticity &&
                   PoissonRatio == other.PoissonRatio &&
                   ThermalCoefficient == other.ThermalCoefficient &&
                   Density == other.Density &&
                   Standard == other.Standard &&
                   Conductivity == other.Conductivity && SpecificHeat == other.SpecificHeat && ViscousDamping == other.ViscousDamping &&
                   CompressiveStrength == other.CompressiveStrength && TensileStrength == other.TensileStrength && TensileYeld == other.TensileYeld;
        }

        public override bool EqualsWithId(object obj)
        {
            return EqualsWithId(obj as MaterialModel);
        }

        public bool EqualsWithId(MaterialModel other)
        {
            return !(other is null) &&
                   base.EqualsWithId(other) &&
                   Name == other.Name &&
                   Type == other.Type &&
                   DampingRatio == other.DampingRatio &&
                   ModulusElasticity == other.ModulusElasticity &&
                   PoissonRatio == other.PoissonRatio &&
                   ThermalCoefficient == other.ThermalCoefficient &&
                   Density == other.Density &&
                   Standard == other.Standard &&
                   Conductivity == other.Conductivity && SpecificHeat == other.SpecificHeat && ViscousDamping == other.ViscousDamping &&
                   CompressiveStrength == other.CompressiveStrength && TensileStrength == other.TensileStrength && TensileYeld == other.TensileYeld;
        }

        public override int GetHashCode()
        {
            int hashCode = 1413619688;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + DampingRatio.GetHashCode();
            hashCode = hashCode * -1521134295 + ModulusElasticity.GetHashCode();
            hashCode = hashCode * -1521134295 + PoissonRatio.GetHashCode();
            hashCode = hashCode * -1521134295 + ThermalCoefficient.GetHashCode();
            hashCode = hashCode * -1521134295 + Density.GetHashCode();
            hashCode = hashCode * -1521134295 + Mass.GetHashCode();
            hashCode = hashCode * -1521134295 + Standard.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(MaterialModel left, MaterialModel right)
        {
            return EqualityComparer<MaterialModel>.Default.Equals(left, right);
        }

        public static bool operator !=(MaterialModel left, MaterialModel right)
        {
            return !(left == right);
        }
    }
}
