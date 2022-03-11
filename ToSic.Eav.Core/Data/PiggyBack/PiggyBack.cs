using System;
using System.Collections.Concurrent;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data.PiggyBack
{
    /// <summary>
    /// Experimental: Goal is to provide a simple value cache to certain objects like AppState
    /// For things which shouldn't be constantly generated / looked up
    /// </summary>
    [PrivateApi("Internal, may still change a lot")]
    public class PiggyBack
    {
        private readonly ConcurrentDictionary<string, object> _cache =
            new ConcurrentDictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

        public bool Has(string key) => _cache.ContainsKey(key);

        public TData GetOrGenerate<TData>(string key, Func<TData> create)
        {
            if (_cache.TryGetValue(key, out var result) && result is TData typed) return typed;
            typed = create();
            _cache.TryAdd(key, typed);
            return typed;
        }
    }
}
