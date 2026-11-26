using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Apps.Sys.Caching;

/// <inheritdoc/>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppsCacheSelector(ISysFeaturesService featuresService, Generator<IAppsCache> appsCache)
    : ServiceBase("Eav.ApChSw", connect: [featuresService]), IAppsCacheSelector
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
        return PreferredKey;
    }

    /// <summary>
    /// WIP - currently here to allow testing overrides.
    /// </summary>
    public static string PreferredKey = AppsCacheInMemory.DefaultNameId;

    #endregion
}
