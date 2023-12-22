using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Data.Source;

/// <summary>
/// WIP 2dm - experimental - need a lookup table that can be updated later on
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LazyLookup<TKey, TValue> : ILookup<TKey, TValue>
{
    public ILookup<TKey, TValue> Source => _source.Get(() => Raw.ToLookup(r => r.Key, r => r.Value));
    private readonly GetOnce<ILookup<TKey, TValue>> _source = new();

    public List<KeyValuePair<TKey, TValue>> Raw = new();

    public LazyLookup(/*ILookup<TKey, TValue> source = null*/)
    {
        //Source = /*source ??*/ Enumerable.Empty<TValue>().ToLookup(x => default(TKey), x => x);
    }

    //public void Update(ILookup<TKey, TValue> source) => Source = source;

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Raw.Add(item);
        _source.Reset();
    }

    public void Add(IEnumerable<KeyValuePair<TKey, TValue>> values)
    {
        Raw.AddRange(values);
        _source.Reset();
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