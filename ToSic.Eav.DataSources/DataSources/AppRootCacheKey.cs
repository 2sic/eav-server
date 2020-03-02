using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.Documentation;

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
        public override string CachePartialKey => _cachePartialKey ?? (_cachePartialKey = base.CachePartialKey + $"&Zone={_appRoot.ZoneId}&App={_appRoot.AppId}");
        private string _cachePartialKey;

        /// <inheritdoc />
        public override string CacheFullKey => CachePartialKey;

    }
}
