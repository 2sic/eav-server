using System.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Caching
{
    /// <summary>
    /// This is a Cache-info wrapper when multiple sources would trigger a cache-refresh
    /// </summary>
    [PrivateApi]
    public class CacheExpiringMultiSource: ICacheExpiring
    {
        private readonly ICacheExpiring[] _sources;

        public CacheExpiringMultiSource(params ICacheExpiring[] sources) => _sources = sources;

        /// <summary>
        /// Assume that the internal timestamp is the largest timestamp available internally
        /// </summary>
        public long CacheTimestamp => _sources.Max(s => s.CacheTimestamp);

        /// <inheritdoc />
        public bool CacheChanged(long newCacheTimeStamp) => CacheTimestamp != newCacheTimeStamp;
    }
}
