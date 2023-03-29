using ToSic.Eav.Apps;
using ToSic.Eav.DataSource.Caching;
using ToSic.Eav.DataSources.Caching;

namespace ToSic.Eav.DataSources
{
    public class AppRootCacheKey: CacheKey
    {
        public AppRootCacheKey(AppRoot appRoot) : base(appRoot)
        {
            _appRoot = appRoot;
        }

        private readonly AppRoot _appRoot;

        /// <inheritdoc />
        public override string CachePartialKey => base.CachePartialKey + AppCacheKey(_appRoot);

        /// <inheritdoc />
        public override string CacheFullKey => CachePartialKey;

        /// <summary>
        /// Add Zone & App IDs to a cache-key
        /// </summary>
        public static string AppCacheKey(IAppIdentity app) => $"&Zone={app.ZoneId}&App={app.AppId}";
    }
}
