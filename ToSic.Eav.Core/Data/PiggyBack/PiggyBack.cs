﻿using System.Collections.Concurrent;
using ToSic.Eav.Caching;

namespace ToSic.Eav.Data.PiggyBack;

/// <summary>
/// Experimental: Goal is to provide a simple value cache to certain objects like AppState
/// For things which shouldn't be constantly generated / looked up
/// </summary>
[PrivateApi("Internal, may still change a lot")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class PiggyBack
{
    private readonly ConcurrentDictionary<string, object> _cache = new(StringComparer.InvariantCultureIgnoreCase);

    public bool Has(string key) => _cache.ContainsKey(key);

    public TData GetOrGenerate<TData>(string key, Func<TData> create)
    {
        if (_cache.TryGetValue(key, out var result) && result is TData typed) return typed;
        try
        {
            typed = create();
            _cache.TryAdd(key, typed);
            return typed;
        }
        catch { /* ignore / silent */ }

        return default;
    }

    public (TData Value, bool IsCached) GetOrGenerate<TData>(ICacheExpiring parent, string key, Func<TData> create)
    {
        // Check if exists and timestamp still ok, return that
        if (_cache.TryGetValue(key, out var result)
            && result is Timestamped<TData> typed
            && typed.CacheTimestamp == parent.CacheTimestamp
           ) return (typed.Value, true);

        // else create it, add timestamp, and store
        try
        {
            var newValue = create();
            var timestamped = new Timestamped<TData>(newValue, parent.CacheTimestamp);
            _cache.TryAdd(key, timestamped);
            return (timestamped.Value, false);
        }
        catch { /* ignore / silent */ }

        return default;
    }
}