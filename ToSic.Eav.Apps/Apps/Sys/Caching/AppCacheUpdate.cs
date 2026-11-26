namespace ToSic.Eav.Apps.Sys.Caching;

[ShowApiWhenReleased(ShowApiMode.Never)]
internal class AppCacheUpdate(IAppsCacheSelector appsCacheSelector) : ServiceBase("App.SysMng"),
    IAppCacheUpdate
{
    /// <inheritdoc/>
    public void Update(IAppIdentity app, IEnumerable<int> entities)
        => appsCacheSelector.Value.Update(app, entities, Log);
}
