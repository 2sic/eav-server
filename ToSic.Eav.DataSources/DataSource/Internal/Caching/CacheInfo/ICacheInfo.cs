using ToSic.Lib.Caching;
using ToSic.Lib.Caching.Keys;

namespace ToSic.Eav.DataSource.Internal.Caching;

/// <summary>
/// This marks an object that can provide everything necessary to
/// provide information about caching
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ICacheInfo: ICacheKey, ICacheExpiring;