using Rhino.Geometry;
using Rhino2Straus.Core.Base;
using System;
using System.Collections.Generic;

namespace Rhino2Straus.Core.Attributes
{
    public class CoordinateSystemModel : ModelObjectId, IEquatable<CoordinateSystemModel>
    {
        public Point3d Origin { get; set; }

        public Point3d Point1 { get; set; }

        public Point3d Point2 { get; set; }

        public readonly static CoordinateSystemModel Global = new CoordinateSystemModel("Global", Point3d.Origin, new Point3d(1, 0, 0), new Point3d(0, 1, 0), 1);
        public readonly static CoordinateSystemModel Local = new CoordinateSystemModel("Local", Point3d.Origin, new Point3d(1, 0, 0), new Point3d(0, 1, 0), 1);

        public CoordinateSystemModel(string name, Point3d origin, Point3d p1, Point3d p2, int id)
            : base(id, name)
        {
            Origin = origin;
            Point1 = p1;
            Point2 = p2;
        }

        public CoordinateSystemModel(CoordinateSystemModel cs)
            : base(cs.Id, cs.Name)
        {
            Origin = cs.Origin;
            Point1 = cs.Point1;
            Point2 = cs.Point2;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CoordinateSystemModel);
        }

        public bool Equals(CoordinateSystemModel other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   Name == other.Name &&
                   Id == other.Id &&
                   Origin.Equals(other.Origin) &&
                   Point1.Equals(other.Point1) &&
                   Point2.Equals(other.Point2);
        }

        public override int GetHashCode()
        {
            int hashCode = -1434879052;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + Origin.GetHashCode();
            hashCode = hashCode * -1521134295 + Point1.GetHashCode();
            hashCode = hashCode * -1521134295 + Point2.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(CoordinateSystemModel left, CoordinateSystemModel right)
        {
            return EqualityComparer<CoordinateSystemModel>.Default.Equals(left, right);
        }

        public static bool operator !=(CoordinateSystemModel left, CoordinateSystemModel right)
        {
            return !(left == right);
        }
    }
}
