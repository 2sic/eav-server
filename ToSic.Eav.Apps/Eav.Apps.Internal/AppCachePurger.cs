using ToSic.Eav.Caching;

namespace ToSic.Eav.Apps.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppCachePurger(IAppStates appStates, AppsCacheSwitch appsCache) : ServiceBase("App.SysMng")
{

    #region purge cache stuff

    /// <summary>
    /// Flush the entire map of zones / apps
    /// </summary>
    public void PurgeZoneList() => Purge(new AppIdentity(0, 0),true);

    /// <summary>
    /// Clear the cache of a specific app/zone, or of everything
    /// </summary>
    /// <param name="zoneId"></param>
    /// <param name="appId"></param>
    /// <param name="global">if true, will flush everything</param>
    public void Purge(int zoneId, int appId, bool global = false) 
        => Purge(new AppIdentity(zoneId, appId), global);

    /// <summary>
    /// Clear the cache of a specific app/zone, or of everything
    /// </summary>
    /// <param name="appIdentity"></param>
    /// <param name="global">if true, will flush everything</param>
    public void Purge(IAppIdentity appIdentity, bool global = false) => Log.Do($"{appIdentity.Show()}, {global}", () =>
    {
        if (global)
            appsCache.Value.PurgeZones();
        else
            appsCache.Value.Purge(appIdentity);
    });

    /// <summary>
    /// Purge the cache of one app
    /// </summary>
    /// <param name="appId"></param>
    public void PurgeApp(int appId) => Log.Do($"{appId}", () => Purge(appStates.AppsCatalog.AppIdentity(appId)));

    /// <summary>
    /// Run some code and then purge the cache after that for full rebuild
    /// </summary>
    public void DoAndPurge(int zoneId, int appId, Action action, bool global = false) => Log.Do($"{zoneId}, {appId}, fn(...), {global}", () =>
    {
        action.Invoke();
        Purge(zoneId, appId, global);
    });

    #endregion

}