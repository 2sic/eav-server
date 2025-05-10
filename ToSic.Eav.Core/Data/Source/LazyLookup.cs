using System.Collections;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Data.Source;

/// <summary>
/// WIP 2dm - experimental - need a lookup table that can be updated later on
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class LazyLookup<TKey, TValue> : ILookup<TKey, TValue>
{
    public ILookup<TKey, TValue> Source
    {
        get => field ??= Raw.ToLookup(r => r.Key, r => r.Value);
        private set => field = value;
    }

    public List<KeyValuePair<TKey, TValue>> Raw = [];

    public void Add(IEnumerable<KeyValuePair<TKey, TValue>> values)
    {
        Raw.AddRange(values);
        Source = null;
    }

    #region IEnumerable
    public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator() => Source.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    #region ILookup

    public bool Contains(TKey key) => Source.Contains(key);

    public int Count => Source.Count;

    public IEnumerable<TValue> this[TKey key] => Source[key];

    #endregion
}