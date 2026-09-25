using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rhino2Straus.Core.Loads
{
    public class AreaGlobalPressureStraus : AreaLoadBaseModel, IEquatable<AreaGlobalPressureStraus>
    {
        public enum LoadFace
        {
            Top,
            Bottom
        }

        public LoadFace Face { get; set; }
        public Vector3d Value { get; set; }
        public double ValueX { get => Value.X; set => Value = new Vector3d(value, Value.Y, Value.Z); }
        public double ValueY { get => Value.Y; set => Value = new Vector3d(Value.X, value, Value.Z); }
        public double ValueZ { get => Value.Z; set => Value = new Vector3d(Value.X, Value.Y, value); }
        public bool Projected { get; set; }

        public AreaGlobalPressureStraus(LoadCaseModel loadCase, LoadFace face, CoordinateSystemModel coordinateSystem, double valueX, double valueY, double valueZ, bool projected = false)
            : base(loadCase)
        {
            Face = face;
            CoordinateSystem = coordinateSystem;
            Value = new Vector3d(valueX, valueY, valueZ);
            Projected = projected;
            CoordinateSystem = CoordinateSystemModel.Global;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AreaGlobalPressureStraus);
        }

        public bool Equals(AreaGlobalPressureStraus other)
        {
            return !(other is null) &&
                   EqualityComparer<LoadCaseModel>.Default.Equals(LoadCase, other.LoadCase) &&
                   Face == other.Face &&
                   EqualityComparer<CoordinateSystemModel>.Default.Equals(CoordinateSystem, other.CoordinateSystem) &&
                   Value.Equals(other.Value) &&
                   Projected == other.Projected;
        }

        public override int GetHashCode()
        {
            int hashCode = -146644460;
            hashCode = hashCode * -1521134295 + EqualityComparer<LoadCaseModel>.Default.GetHashCode(LoadCase);
            hashCode = hashCode * -1521134295 + Face.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<CoordinateSystemModel>.Default.GetHashCode(CoordinateSystem);
            hashCode = hashCode * -1521134295 + Value.GetHashCode();
            hashCode = hashCode * -1521134295 + Projected.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(AreaGlobalPressureStraus left, AreaGlobalPressureStraus right)
        {
            return EqualityComparer<AreaGlobalPressureStraus>.Default.Equals(left, right);
        }

        public static bool operator !=(AreaGlobalPressureStraus left, AreaGlobalPressureStraus right)
        {
            return !(left == right);
        }
    }
}
