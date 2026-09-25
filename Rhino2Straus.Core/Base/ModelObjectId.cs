using System;
using System.Collections.Generic;

namespace Rhino2Straus.Core.Base
{
    public class ModelObjectId : IEquatable<ModelObjectId>
    {
        public const int UNASSIGNED = -1;

        public string Name { get; set; }

        public int Id { get; set; }

        public ModelObjectId()
        {
            Name = string.Empty;
            Id = UNASSIGNED;
        }

        public ModelObjectId(string name)
        {
            Name = name;
            Id = UNASSIGNED;
        }

        public ModelObjectId(int id, string name)
        {
            Name = name;
            Id = id;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ModelObjectId);
        }

        public virtual bool Equals(ModelObjectId other)
        {
            return !(other is null) &&
                   Name == other.Name;
        }

        public virtual bool EqualsWithId(object obj)
        {
            return EqualsWithId(obj as ModelObjectId);
        }

        public virtual bool EqualsWithId(ModelObjectId other)
        {
            return !(other is null) &&
                   Name == other.Name &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }

        public static bool operator ==(ModelObjectId left, ModelObjectId right)
        {
            return EqualityComparer<ModelObjectId>.Default.Equals(left, right);
        }

        public static bool operator !=(ModelObjectId left, ModelObjectId right)
        {
            return !(left == right);
        }
    }
}
