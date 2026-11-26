namespace ToSic.Eav.Apps.Sys.Caching;

// Note: this should be internal, but ATM can't be
// Because it's used in 2sxc System Tests in the WebApi,
// and we don't want to make internals visible to that?

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppCachePurge(AppsCacheSwitch appsCacheSelector) : ServiceBase("App.SysMng"),
    IAppCachePurge
{
    /// <inheritdoc/>
    public void Purge(IAppIdentity app)
        => appsCacheSelector.Value.Purge(app);

}
