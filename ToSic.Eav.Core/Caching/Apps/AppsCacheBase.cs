using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.DI;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Caching
{
    /// <summary>
    /// The Apps Cache is the main cache for App States. <br/>
    /// This is just the abstract base implementation.
    /// The real cache must implement this and also provide platform specific adjustments so that the caching is in sync with the Environment.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public abstract class AppsCacheBase : IAppsCacheSwitchable
    {
        // TODO: WILL PROBABLY need an update on the farm cache which doesn't have these dependencies yet

        #region Switchable

        public virtual string NameId => "Base";

        public virtual bool IsViable() => true;

        public virtual int Priority => 0;

        #endregion

        /// <summary>
        /// The repository loader. Must generate a new one on every access, to be sure that it doesn't stay in memory for long. 
        /// </summary>
        private IRepositoryLoader GetNewRepoLoader(IServiceProvider sp) => sp.Build<IRepositoryLoader>();

        public abstract IReadOnlyDictionary<int, Zone> Zones(IServiceProvider sp);

        [PrivateApi]
        protected IReadOnlyDictionary<int, Zone> LoadZones(IServiceProvider sp)
        {
            // Load from DB (this will also ensure that Primary Apps are created)
            var realZones = GetNewRepoLoader(sp).Zones();

            // Add the Preset-Zone to the list - still WIP v13
            try
            {
                var presetZone = new Zone(Constants.PresetZoneId,
                    Constants.PresetAppId,
                    Constants.PresetAppId,
                    new Dictionary<int, string> { { Constants.PresetAppId, Constants.PresetName } },
                    new List<Data.DimensionDefinition>()
                    {
                        new Data.DimensionDefinition()
                        {
                            Active = true,
                            DimensionId = 0,
                            EnvironmentKey = "en-us",
                            Key = "en-us",
                            Name = "English",
                            Parent = null
                        }
                    });
                realZones.Add(Constants.PresetZoneId, presetZone);
            }
            catch { /* ignore */ }

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
        protected abstract void Set(string key, AppState item);

        /// <summary>
        /// Get CacheItem with specified CacheKey
        /// </summary>
        [PrivateApi("only important for developers, and they have intellisense")]
		protected abstract AppState Get(string key);

        /// <summary>
        /// Remove the CacheItem with specified CacheKey
        /// </summary>
        [PrivateApi("only important for developers, and they have intellisense")]
		protected abstract void Remove(string key);

        [PrivateApi]
        public void Add(AppState appState) => Set(CacheKey(appState), appState);

        #endregion

        /// <inheritdoc />
        public AppState Get(IServiceProvider sp, IAppIdentity app) => GetOrBuild(sp, app);


        /// <inheritdoc />
        public void Load(IServiceProvider sp, IAppIdentity app, string primaryLanguage) => GetOrBuild(sp, app, primaryLanguage);


        private AppState GetOrBuild(IServiceProvider sp, IAppIdentity appIdentity, string primaryLanguage = null)
        {
            if (appIdentity.ZoneId == 0 || appIdentity.AppId == Constants.AppIdEmpty)
                return null;

            var cacheKey = CacheKey(appIdentity);

            AppState appState = null;
            if (Has(cacheKey)) appState = Get(cacheKey);
            if (appState != null) return appState;

            // create lock to prevent parallel initialization
            var lockKey = LoadLocks.GetOrAdd(cacheKey, new object());
            lock (lockKey)
            {
                // now that lock is free, it could have been initialized, so re-check
                if (Has(cacheKey)) appState = Get(cacheKey);
                if (appState != null) return appState;

                // Init EavSqlStore once
                var loader = GetNewRepoLoader(sp);
                if (primaryLanguage != null) loader.PrimaryLanguage = primaryLanguage;
                appState = loader.AppState(appIdentity.AppId, true);
                Set(cacheKey, appState);
            }

            return appState; // Get(cacheKey);
        }

        /// <summary>
        /// List of locks, to ensure that each app locks the loading process separately
        /// </summary>
        private static readonly ConcurrentDictionary<string, object> LoadLocks 
            = new ConcurrentDictionary<string, object>();

        #region Purge Cache

        /// <inheritdoc />
        public void Purge(IAppIdentity app) => Remove(CacheKey(app));

	    /// <inheritdoc />
	    public abstract void PurgeZones();

        #endregion

        #region Update

        /// <inheritdoc />
        public virtual AppState Update(IServiceProvider sp, IAppIdentity app, IEnumerable<int> entities, ILog log)
        {
            var wrapLog = log.Fn<AppState>();
            // if it's not cached yet, ignore the request as partial update won't be necessary
            if (!Has(app)) return wrapLog.ReturnNull("not cached, won't update");
            var appState = Get(sp, app);
            GetNewRepoLoader(sp).Init(log).Update(appState, AppStateLoadSequence.ItemLoad, entities.ToArray());
            return wrapLog.ReturnAsOk(appState);
        }

        #endregion


        [PrivateApi]
        public int ZoneIdOfApp(IServiceProvider sp, int appId)
        {
            try
            {
                var zones = Zones(sp);
                // var zone = zones.FirstOrDefault(z => z.Value.Apps.Any(a => a.Key == appId));
                return zones.Single(z => z.Value.Apps.Any(a => a.Key == appId)).Key;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error trying to run {nameof(ZoneIdOfApp)}({appId}) - probably something wrong with the {nameof(appId)}", ex);
            }
        }

    }
}
