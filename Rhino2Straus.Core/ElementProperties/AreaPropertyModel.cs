using Rhino2Straus.Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Rhino2Straus.Core.ElementProperties
{
    public class AreaPropertyModel : ModelObjectId, IEquatable<AreaPropertyModel>
    {
        public enum PlatePropertyType { Undefined=-1, PlateShell=0, Membrane=4, LoadPatch=5, ShearPanel=6, PlaneStress=7, PlaneStrain=8, Axisymmetric=9 }

        public MaterialModel Material { get; set; }
        public AreaThicknessModel Section { get; set; }
        public PlatePropertyType PropertyType { get; set; }

        public double ThicknessMembrane => Section.ThicknessMembrane;
        public double ThicknessBending => Section.ThicknessBending;
        public double Offset => Section.Offset;

        public AreaPropertyModel(string name, MaterialModel material, AreaThicknessModel section, PlatePropertyType propertyType = PlatePropertyType.Undefined)
            : base(name)
        {
            Material = material;
            Section = section;
            PropertyType = propertyType;
        }

        public AreaPropertyModel(AreaPropertyModel areaPropertyModel)
            : this(areaPropertyModel.Name, areaPropertyModel.Material, areaPropertyModel.Section, areaPropertyModel.PropertyType)
        {
            Id = areaPropertyModel.Id;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AreaPropertyModel);
        }

        public bool Equals(AreaPropertyModel other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   Name == other.Name &&
                   PropertyType == other.PropertyType &&
                   EqualityComparer<MaterialModel>.Default.Equals(Material, other.Material) &&
                   EqualityComparer<AreaThicknessModel>.Default.Equals(Section, other.Section);
        }

        public override int GetHashCode()
        {
            int hashCode = -1663097784;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + PropertyType.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<MaterialModel>.Default.GetHashCode(Material);
            hashCode = hashCode * -1521134295 + EqualityComparer<AreaThicknessModel>.Default.GetHashCode(Section);
            return hashCode;
        }

        public static bool operator ==(AreaPropertyModel left, AreaPropertyModel right)
        {
            return EqualityComparer<AreaPropertyModel>.Default.Equals(left, right);
        }

        public static bool operator !=(AreaPropertyModel left, AreaPropertyModel right)
        {
            return !(left == right);
        }
    }
}
