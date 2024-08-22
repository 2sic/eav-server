using ToSic.Eav.Apps.State;

namespace ToSic.Eav.Apps;

[PrivateApi("WIP")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IAppStates
{
    #region Get an App

    /// <summary>
    /// Retrieve an app from the cache
    /// </summary>
    /// <param name="app">App identifier.</param>
    IAppStateCache Get(IAppIdentity app);

    /// <summary>
    /// Retrieve an app from the cache
    /// </summary>
    /// <param name="appId">App id if zone unknown.</param>
    IAppStateCache GetCacheState(int appId);

    #endregion

    #region Zone Stuff

    IAppsCatalog AppsCatalog { get; }

    //IDictionary<int, string> Apps(int zoneId);

    //List<DimensionDefinition> Languages(int zoneId, bool includeInactive = false);

    #endregion

    bool IsCached(IAppIdentity appId);
}
