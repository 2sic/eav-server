using ToSic.Eav.Apps.State;
using ToSic.Eav.Data;

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

    IAppStateInternal ToReader(IAppStateCache state, ILog log = default);

    #endregion

    #region Look up IDs

    IAppIdentityPure IdentityOfApp(int appId);

    IAppIdentityPure IdentityOfPrimary(int zoneId);

    IAppIdentityPure IdentityOfDefault(int zoneId);

    string AppIdentifier(int zoneId, int appId);

    #endregion

    #region Zone Stuff

    int DefaultAppId(int zoneId);

    int PrimaryAppId(int zoneId);


    IDictionary<int, string> Apps(int zoneId);

    List<DimensionDefinition> Languages(int zoneId, bool includeInactive = false);

    IReadOnlyDictionary<int, Zone> Zones { get; }

    #endregion

    bool IsCached(IAppIdentity appId);
}
