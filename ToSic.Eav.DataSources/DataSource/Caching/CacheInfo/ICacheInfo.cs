using ToSic.Eav.Caching;

namespace ToSic.Eav.DataSource.Caching.CacheInfo;

/// <summary>
/// This marks an object that can provide everything necessary to
/// provide information about caching
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ICacheInfo: ICacheKey, ICacheExpiring
{
}