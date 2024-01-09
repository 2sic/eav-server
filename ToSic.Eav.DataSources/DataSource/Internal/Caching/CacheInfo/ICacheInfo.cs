using ToSic.Eav.Caching;

namespace ToSic.Eav.DataSource.Internal.Caching;

/// <summary>
/// This marks an object that can provide everything necessary to
/// provide information about caching
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ICacheInfo: ICacheKey, ICacheExpiring;