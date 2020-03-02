using System;
using System.Collections;
using System.Collections.Generic;

namespace ToSic.Eav.Generics
{
    public class DictionaryInvariant<T>: IDictionary<string, T>
    {
        public DictionaryInvariant()
        {
            Original = new Dictionary<string, T>(comparer: StringComparer.InvariantCultureIgnoreCase);
        }

        public DictionaryInvariant(IDictionary<string, T> original)
        {
            Original = original;
        }

        protected readonly IDictionary<string, T> Original;
            

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator() => Original.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Original.GetEnumerator();

        public void Add(KeyValuePair<string, T> item) => Original.Add(item);

        public void Clear() => Original.Clear();

        public bool Contains(KeyValuePair<string, T> item) => Original.Contains(item);

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex) => Original.CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, T> item) => Original.Remove(item);

        public int Count => Original.Count;

        public bool IsReadOnly => Original.IsReadOnly;

        public bool ContainsKey(string key) => Original.ContainsKey(key);

        public void Add(string key, T value) => Original.Add(key, value);

        public bool Remove(string key) => Original.Remove(key);

        public bool TryGetValue(string key, out T value) => Original.TryGetValue(key, out value);

        public T this[string key]
        {
            get => Original[key];
            set => Original[key] = value;
        }

        public ICollection<string> Keys => Original.Keys;

        public ICollection<T> Values => Original.Values;
    }
}
