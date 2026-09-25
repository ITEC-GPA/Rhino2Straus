using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Rhino2Straus.Core.Base;

namespace Rhino2Straus.Core.Collections
{
    public class UniqueIdCollection<T> : Dictionary<int, T>, IEquatable<UniqueIdCollection<T>> where T : ModelObjectId
    {
        #region Variables

        protected int _maxId;

        #endregion

        #region Properties

        public int MaxId { get => _maxId; protected set => _maxId = value; }

        #endregion

        #region Constructor

        public UniqueIdCollection()
            : base()
        {
            _maxId = 0;
        }


        #endregion

        #region Add

        public new void Clear() { base.Clear(); _maxId=0; }

        public bool Add(T item)
        {
            if (item.Id <= ElementModel.UNASSIGNED)
            {
                item.Id = ++_maxId;
                Add(item.Id, item);
                return true;
            }
            else
            {
                if (ContainsKey(item.Id))
                {
                    this[item.Id] = item;
                    return true;
                }

                if (item.Id > _maxId)
                    _maxId = item.Id;

                Add(item.Id, item);
                return true;
            }
        }

        public bool AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                if (!Add(item))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion


        #region Methos


        public override bool Equals(object obj)
        {
            return obj is UniqueIdCollection<T> collection && collection.SequenceEqual(this);
        }

        public bool Equals(UniqueIdCollection<T> other)
        {
            return !(other is null) &&
                EqualityComparer<IEqualityComparer<int>>.Default.Equals(Comparer, other.Comparer) &&
                Count == other.Count &&
                Keys.SequenceEqual(other.Keys) &&
                Values.SequenceEqual(other.Values) &&
                _maxId == other._maxId;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + EqualityComparer<IEqualityComparer<int>>.Default.GetHashCode(Comparer);
                hashCode = hashCode * -17 + Count.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<KeyCollection>.Default.GetHashCode(Keys);
                hashCode = hashCode * -17 + EqualityComparer<ValueCollection>.Default.GetHashCode(Values);
                hashCode = hashCode * -17 + MaxId.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(UniqueIdCollection<T> left, UniqueIdCollection<T> right)
        {
            return EqualityComparer<UniqueIdCollection<T>>.Default.Equals(left, right);
        }

        public static bool operator !=(UniqueIdCollection<T> left, UniqueIdCollection<T> right)
        {
            return !(left == right);
        }

        #endregion
    }
}
