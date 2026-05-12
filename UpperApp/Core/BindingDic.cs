using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;

namespace UpperApp.Core
{
    internal class BindingDic<T> where T : class
    {
        private readonly ConcurrentDictionary<string, T> ConnectDic = [];
        public readonly BindingList<string> connectionKeys;

        public int Count => ConnectDic.Count;

        public bool TryGet(string key, out T value)
        {
            return ConnectDic.TryGetValue(key, out value);
        }

        public BindingDic()
        {
            connectionKeys = new BindingList<string>([.. ConnectDic.Keys]);
        }

        public void Add(string name, T obj)
        {
            if (ConnectDic.TryAdd(name, obj))
            {
                connectionKeys.Add(name);
            }
        }

        public T Remove(string name)
        {
            if (ConnectDic.TryRemove(name, out T obj))
            {
                connectionKeys.Remove(name);
                return obj;
            }
            return null;
        }
    }
}
