using System.Collections.Generic;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Data;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

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

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class IAppStatesExtensions
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IAppState KeepOrGetReader(this IAppStates appStates, IAppIdentity app, ILog log = default)
        => (app is IAppStateCache stateCache ? appStates.ToReader(stateCache) : null)
           ?? app as IAppState ?? appStates.GetReader(app);

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IAppState KeepOrGetReader(this LazySvc<IAppStates> appStates, IAppIdentity app) 
        => app as IAppState ?? appStates.Value.GetReader(app);
}