using System.Collections.Concurrent;

namespace UpperApp.Core
{
    internal class BindingDic<T> where T : class
    {
        private readonly ConcurrentDictionary<string, T> ConnectDic = [];

        public int Count => ConnectDic.Count;

        /// <summary>当前连接键的快照数组。</summary>
        public string[] Keys => [.. ConnectDic.Keys];

        public bool TryGet(string key, out T value)
        {
            return ConnectDic.TryGetValue(key, out value);
        }

        public void Add(string name, T obj)
        {
            ConnectDic.TryAdd(name, obj);
        }

        public T Remove(string name)
        {
            return ConnectDic.TryRemove(name, out T obj) ? obj : null;
        }
    }
}
