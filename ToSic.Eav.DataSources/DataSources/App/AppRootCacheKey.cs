using ToSic.Eav.Apps;
using ToSic.Eav.DataSource.Internal.Caching;

namespace ToSic.Eav.DataSources;

internal class AppRootCacheKey(AppRoot appRoot) : CacheKey(appRoot)
{
    /// <inheritdoc />
    public override string CachePartialKey => base.CachePartialKey + AppCacheKey(appRoot);

    /// <inheritdoc />
    public override string CacheFullKey => CachePartialKey;

    /// <summary>
    /// Add Zone & App IDs to a cache-key
    /// </summary>
    public static string AppCacheKey(IAppIdentity app) => $"&Zone={app.ZoneId}&App={app.AppId}";
}