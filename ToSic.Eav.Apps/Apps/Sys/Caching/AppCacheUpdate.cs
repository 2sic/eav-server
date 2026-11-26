using ToSic.Eav.Apps.Sys.Loaders;

namespace ToSic.Eav.Apps.Sys.Caching;

[ShowApiWhenReleased(ShowApiMode.Never)]
internal class AppCacheUpdate(IAppsCacheSelector appsCacheSelector, LazySvc<IAppLoaderTools> appLoaderTools) : ServiceBase("App.SysMng"),
    IAppCacheUpdate
{
    /// <inheritdoc/>
    public void Update(IAppIdentity app, IEnumerable<int> entities)
        => appsCacheSelector.Value.Update(app, entities, Log, appLoaderTools.Value);
}
