using System.Collections.Generic;
using System.Runtime.Serialization;
using Rhino2Straus.Core.Base;

namespace Rhino2Straus.Core.Collections
{
    public class UniqueNameCollection<T> : Dictionary<string, T> where T : ModelObjectId
    {
        public UniqueNameCollection()
        {
        }


        public void Add(T item)
        {
            if(ContainsKey(item.Name))
                this[item.Name] = item;
            else
                Add(item.Name, item);
        }

        public bool AddRange(IEnumerable<T> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    Add(item);
                }
                return true;
            }
            return false;
        }

        public Dictionary<int, T> GetDictionary()
        {
            Dictionary<int, T> idAss = new Dictionary<int, T>();
            foreach (var item in this)
            {
                idAss.Add(item.Value.Id, item.Value);
            }
            return idAss;
        }

        public T GetByID(int id)
        {
            foreach (var item in this)
                if (item.Value.Id == id)
                    return item.Value;

            return null;
        }
    }
}
