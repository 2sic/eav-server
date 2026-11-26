using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Apps.Sys.Caching;

/// <summary>
/// This will retrieve the current AppCache.
/// Basically it will get all registered <see cref="IAppsCache"/> and get the first which says it's valid.
/// Once it's found the right one, it will keep returning that,
/// unless the features report that a setting has been changed, in which case it will re-evaluate the best match. 
/// </summary>
/// <remarks>
/// This class is transient, but it uses a static cache for the value, so the returned value behaves as a "safe" singleton.
/// </remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppsCacheSwitch(
    ISysFeaturesService featuresService,
    Generator<IAppsCache> appsCache,
    LazySvc<IAppLoaderTools> appLoaderTools)
    : ServiceBase("Eav.ApChSw", connect: [featuresService])
{
    #region Main Value Handling (access static presolved value, optionally reset)

    /// <summary>
    /// Get the current AppsCache - using the name/key from the settings
    /// so that it could be overloaded.
    /// </summary>
    public IAppsCache Value => field
        ??= appsCache.New(GetPreferredKey());

    /// <summary>
    /// TODO: change this to use the key as provided by the featuresService
    /// </summary>
    /// <returns></returns>
    private string GetPreferredKey()
    {
        return AppsCache.DefaultNameId;
    }

    #endregion

    public void Update(IAppIdentity app, IEnumerable<int> entities)
        => Value.Update(app, entities, Log, appLoaderTools.Value);

}
