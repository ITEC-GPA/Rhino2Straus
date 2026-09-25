using System;
using System.Collections.Generic;
using Rhino2Straus.Core.Base;

namespace Rhino2Straus.Core.ElementProperties
{
    public class AreaThicknessModel : ModelObjectId, IEquatable<AreaThicknessModel>
    {
        public double ThicknessMembrane { get; set; }
        public double ThicknessBending { get; set; }

        public double Offset { get; set; }

        public AreaThicknessModel(string name, double thicknessMembrane, double thicknessBending, double offset = 0)
            : base(name)
        {
            ThicknessMembrane = thicknessMembrane;
            ThicknessBending = thicknessBending;
            Offset = offset;
        }

        public AreaThicknessModel(string name, double thicknessMembrane, double offset = 0)
            : this(name, thicknessMembrane, 0, offset)
        {
            ThicknessMembrane = thicknessMembrane;
            ThicknessBending = thicknessMembrane;
            Offset = offset;
        }

        public AreaThicknessModel()
            :base()
        {
        }

        public AreaThicknessModel(AreaThicknessModel areaThicknessModel)
            :base(areaThicknessModel.Id, areaThicknessModel.Name)
        {
            ThicknessMembrane = areaThicknessModel.ThicknessMembrane;
            ThicknessBending = areaThicknessModel.ThicknessBending;
            Offset = areaThicknessModel.Offset;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AreaThicknessModel);
        }

        public bool Equals(AreaThicknessModel other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   Name == other.Name &&
                   ThicknessMembrane == other.ThicknessMembrane &&
                   ThicknessBending == other.ThicknessBending &&
                   Offset == other.Offset;
        }

        public override int GetHashCode()
        {
            int hashCode = 475748675;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + ThicknessMembrane.GetHashCode();
            hashCode = hashCode * -1521134295 + Offset.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(AreaThicknessModel left, AreaThicknessModel right)
        {
            return EqualityComparer<AreaThicknessModel>.Default.Equals(left, right);
        }

        public static bool operator !=(AreaThicknessModel left, AreaThicknessModel right)
        {
            return !(left == right);
        }
    }
}
