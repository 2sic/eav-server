using System.Collections.Generic;
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
    AppState Get(IAppIdentity app);

    /// <summary>
    /// Retrieve an app from the cache
    /// </summary>
    /// <param name="appId">App id if zone unknown.</param>
    AppState Get(int appId);

    #endregion

    #region Look up IDs

    IAppIdentity IdentityOfApp(int appId);

    IAppIdentity IdentityOfPrimary(int zoneId);
    IAppIdentity IdentityOfDefault(int zoneId);

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
    //[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    //public static AppState KeepOrGetStateCache(this IAppStates appStates, IAppIdentity app) => app as AppState ?? appStates.Get(app);

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IAppState KeepOrGetReader(this IAppStates appStates, IAppIdentity app, ILog log = default)
    {
        return (app is AppState stateCache ? stateCache.ToInterface(log) : null)
            ?? app as IAppState ?? appStates.GetReader(app);
    }

    //[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    //public static AppState KeepOrGetStateCache(this LazySvc<IAppStates> appStates, IAppIdentity app) 
    //    => app as AppState ?? (app is IAppStateInternal appReadInt ? appReadInt.StateCache : null) ?? appStates.Value.Get(app);

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IAppState KeepOrGetReader(this LazySvc<IAppStates> appStates, IAppIdentity app) 
        => app as IAppState ?? appStates.Value.GetReader(app);
}