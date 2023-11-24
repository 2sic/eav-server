using System.Linq;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Caching;

/// <summary>
/// This is a Cache-info wrapper when multiple sources would trigger a cache-refresh
/// </summary>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class CacheExpiringMultiSource: ICacheExpiring
{
    private readonly ITimestamped[] _sources;

    public CacheExpiringMultiSource(params ITimestamped[] sources) => _sources = sources;

    /// <summary>
    /// Assume that the relevant timestamp is the largest timestamp available on any of the sources.
    /// </summary>
    public long CacheTimestamp => _sources.Max(s => s.CacheTimestamp);

    /// <inheritdoc />
    public bool CacheChanged(long dependentTimeStamp) => CacheTimestamp != dependentTimeStamp;
}