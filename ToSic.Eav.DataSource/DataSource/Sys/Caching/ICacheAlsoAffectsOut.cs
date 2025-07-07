namespace ToSic.Eav.DataSource.Sys.Caching;

/// <summary>
/// Special marker interface to indicate that when resetting the cache, it should also reset the Out, not just the In
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
internal interface ICacheAlsoAffectsOut;