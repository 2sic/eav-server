using System.Collections.ObjectModel;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Persistence.Sys.AppState;
using ToSic.Eav.Sys;
using ToSic.Sys.Locking;

namespace ToSic.Eav.Apps.Sys.Caching;

/// <summary>
/// The Apps Cache is the main cache for App States. <br/>
/// This is just the abstract base implementation.
/// The real cache must implement this and also provide platform specific adjustments so that the caching is in sync with the Environment.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class AppsCacheBase : IAppsCacheSwitchable
{
    #region Switchable

    public virtual string NameId => "Base";

    public virtual bool IsViable() => true;

    public virtual int Priority => 0;

    #endregion

    public abstract IReadOnlyDictionary<int, Zone> Zones(IAppLoaderTools tools);

    [PrivateApi]
    protected IReadOnlyDictionary<int, Zone> LoadZones(IAppLoaderTools tools)
    {
        // Load from DB (this will also ensure that Primary Apps are created)
        var realZones = tools.RepositoryLoader(null).Zones();

        // Add the Preset-Zone to the list - important, otherwise everything fails
        var presetZone = new Zone(KnownAppsConstants.PresetZoneId,
            KnownAppsConstants.PresetAppId,
            KnownAppsConstants.PresetAppId,
            new ReadOnlyDictionary<int, string>(
                new Dictionary<int, string>
                {
                    { KnownAppsConstants.PresetAppId, KnownAppsConstants.PresetName }
                }),
            [
                new()
                {
                    Active = true,
                    DimensionId = 0,
                    EnvironmentKey = "en-us",
                    Key = "en-us",
                    Name = "English",
                    Parent = null
                }
            ]);
        realZones.Add(KnownAppsConstants.PresetZoneId, presetZone);

        return new ReadOnlyDictionary<int, Zone>(realZones);
    }

    #region Cache-Keys

    /// <summary>
    /// Gets the KeySchema used to store values for a specific Zone and App. Must contain {0} for ZoneId and {1} for AppId
    /// </summary>
    [PrivateApi]
    public virtual string CacheKeySchema { get; protected set; } = "Z{0}A{1}";

    [PrivateApi]
    protected string CacheKey(IAppIdentity appIdentity) => string.Format(CacheKeySchema, appIdentity.ZoneId, appIdentity.AppId);

    #endregion



    /// <inheritdoc />
    public bool Has(IAppIdentity app) => Has(CacheKey(app));

    #region Definition of the abstract Has, Set, Get, Remove

    /// <summary>
    /// Test whether CacheKey exists in the Cache
    /// </summary>
    [PrivateApi("only important for developers, and they have intellisense")]
    protected abstract bool Has(string cacheKey);

    /// <summary>
    /// Sets the CacheItem with specified CacheKey
    /// </summary>
    [PrivateApi("only important for developers, and they have intellisense")]
    protected abstract void Set(string key, IAppStateCache item);

    /// <summary>
    /// Get CacheItem with specified CacheKey
    /// </summary>
    [PrivateApi("only important for developers, and they have intellisense")]
    protected abstract IAppStateCache Get(string key);

    /// <summary>
    /// Remove the CacheItem with specified CacheKey
    /// </summary>
    [PrivateApi("only important for developers, and they have intellisense")]
    protected abstract void Remove(string key);

    [PrivateApi]
    public void Add(IAppStateCache appState) => Set(CacheKey(appState), appState);

    #endregion

    /// <inheritdoc />
    public IAppStateCache Get(IAppIdentity app, IAppLoaderTools tools)
        => GetOrBuild(tools, app);


    /// <inheritdoc />
    public void Load(IAppIdentity app, string primaryLanguage, IAppLoaderTools tools)
        => GetOrBuild(tools, app, primaryLanguage);


    private IAppStateCache GetOrBuild(IAppLoaderTools tools, IAppIdentity appIdentity, string primaryLanguage = null)
    {
        if (appIdentity.ZoneId == 0 || appIdentity.AppId == KnownAppsConstants.AppIdEmpty)
            return null;

        var cacheKey = CacheKey(appIdentity);

        IAppStateCache appState = null;
        if (Has(cacheKey)) appState = Get(cacheKey);
        if (appState != null)
            return appState;

        // create lock to prevent parallel initialization
        var lockKey = LoadLocks.Get(cacheKey);
        lock (lockKey)
        {
            // now that lock is free, it could have been initialized, so re-check
            if (Has(cacheKey)) appState = Get(cacheKey);
            if (appState != null)
                return appState;

            // Init EavSqlStore once
            var loader = tools.RepositoryLoader(null);
            if (primaryLanguage != null)
                loader.PrimaryLanguage = primaryLanguage;
            appState = loader.AppState(appIdentity.AppId, new CodeRefTrail().AddMessage($"App: {cacheKey}; DateTime: {DateTime.Now:O}; 2sxc Version: {EavSystemInfo.VersionString}"));
            Set(cacheKey, appState);
        }

        return appState;
    }

    /// <summary>
    /// List of locks, to ensure that each app locks the loading process separately
    /// </summary>
    private static readonly NamedLocks LoadLocks = new();

    #region Purge Cache

    /// <inheritdoc />
    public void Purge(IAppIdentity app)
    {
        var key = CacheKey(app);
        if (Has(key)) Get(key)?.PreRemove();
        Remove(key);
    }

    /// <inheritdoc />
    public abstract void PurgeZones();

    #endregion

    #region Update

    /// <inheritdoc />
    public virtual IAppStateCache Update(IAppIdentity appIdentity, IEnumerable<int> entities, ILog log, IAppLoaderTools tools)
    {
        var l = log.Fn<IAppStateCache>($"{appIdentity}, {entities.Count()} entities");
        // if it's not cached yet, ignore the request as partial update won't be necessary
        if (!Has(appIdentity)) return l.ReturnNull("not cached, won't update");
        var appState = Get(appIdentity, tools);
        tools.RepositoryLoader(log).Update(appState, AppStateLoadSequence.ItemLoad, new(), entities.ToArray());
        return l.ReturnAsOk(appState);
    }

    #endregion


    [PrivateApi]
    public int ZoneIdOfApp(int appId, IAppLoaderTools tools)
    {
        try
        {
            var zones = Zones(tools);
            return zones.Single(z => z.Value.Apps.Any(a => a.Key == appId)).Key;
        }
        catch (Exception ex)
        {
            throw new($"Error trying to run {nameof(ZoneIdOfApp)}({appId}) - probably something wrong with the {nameof(appId)}", ex);
        }
    }

}