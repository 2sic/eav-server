using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Data.Source
{
    /// <summary>
    /// WIP 2dm - experimental - need a lookup table that can be updated later on
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class LazyLookup<TKey, TValue>: ILookup<TKey, TValue>
    {
        public ILookup<TKey, TValue> Source { get; private set; }

        public LazyLookup(ILookup<TKey, TValue> source = null)
        {
            Source = source ?? Enumerable.Empty<TValue>().ToLookup(x => default(TKey), x => x);
        }

        public void Update(ILookup<TKey, TValue> source) => Source = source;

        public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator() => Source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Contains(TKey key) => Source.Contains(key);

        public int Count => Source.Count;

        public IEnumerable<TValue> this[TKey key] => Source[key];
    }
}
