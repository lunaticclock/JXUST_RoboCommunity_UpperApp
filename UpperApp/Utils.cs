using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace UpperApp
{
    class BindingDic<T>
    {
        private readonly Dictionary<string, T> ConnectDic = [];
        public readonly BindingList<string> connectionKeys;
        public T this[string index]
        {
            get
            {
                return ConnectDic[index];
            }
            set
            {
                ConnectDic[index] = value;
            }
        }
        public int Count
        {
            get { return ConnectDic.Count; }
        }

        public BindingDic()
        {
            connectionKeys = new BindingList<string>(ConnectDic.Keys.ToList());
        }

        public void Add(string name, T obj)
        {
            ConnectDic.Add(name, obj);
            connectionKeys.Add(name);
        }

        public T Remove(string name)
        {
            T obj = ConnectDic[name];
            ConnectDic.Remove(name);
            connectionKeys.Remove(name);
            return obj;
        }

        public bool Remove(T connect)
        {
            List<string> keys = ConnectDic.Where(pairs => pairs.Value.Equals(connect)).Select(pairs => pairs.Key).ToList();
            if (keys.Count > 0)
            {
                ConnectDic.Remove(keys[0]);
                connectionKeys.Remove(keys[0]);
                return true;
            }
            else
                return false;
        }

        public T Get(string name)
        {
            return ConnectDic[name];
        }
    }
}
