using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Caching
{
    /// <summary>
    /// The Apps Cache is the main cache for App States. <br/>
    /// This is just the abstract base implementation.
    /// The real cache must implement this and also provide platform specific adjustments so that the caching is in sync with the Environment.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public abstract class AppsCacheBase : IAppsCache
    {
        #region EnforceSingleton experimental

        [PrivateApi] public virtual bool EnforceSingleton => false;
        #endregion

        /// <summary>
        /// The repository loader. Must generate a new one on every access, to be sure that it doesn't stay in memory for long. 
        /// </summary>
        private IRepositoryLoader GetNewRepoLoader() => Factory.StaticBuild<IRepositoryLoader>();

	    /// <inheritdoc />
	    public abstract IReadOnlyDictionary<int, Zone> Zones { get; }

        [PrivateApi]
        protected IReadOnlyDictionary<int, Zone> LoadZones() => GetNewRepoLoader().Zones();

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

        #endregion

        /// <inheritdoc />
        public AppState Get(IAppIdentity app) => GetOrBuild(app);

        /// <inheritdoc />
        public AppState Get(int appId) => Get(GetIdentity(null, appId));


        /// <inheritdoc />
        public void Load(IAppIdentity app, string primaryLanguage) => GetOrBuild(app, primaryLanguage);


        private AppState GetOrBuild(IAppIdentity appIdentity, string primaryLanguage = null)
        {
            if (appIdentity.ZoneId == 0 || appIdentity.AppId == Constants.AppIdEmpty)
                return null;

            var cacheKey = CacheKey(appIdentity);

            if (Has(cacheKey)) return Get(cacheKey);

            // create lock to prevent parallel initialization
            var lockKey = LoadLocks.GetOrAdd(cacheKey, new object());
            lock (lockKey)
            {
                // now that lock is free, it could have been initialized, so re-check
                if (Has(cacheKey)) return Get(cacheKey);

                // Init EavSqlStore once
                var loader = GetNewRepoLoader();
                if (primaryLanguage != null) loader.PrimaryLanguage = primaryLanguage;
                var appState = loader.AppState(appIdentity.AppId, true);
                Set(cacheKey, appState);
            }

            return Get(cacheKey);
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
        public virtual AppState Update(IAppIdentity app, IEnumerable<int> entities, ILog log)
        {
            var wrapLog = log.Call<AppState>();
            // if it's not cached yet, ignore the request as partial update won't be necessary
            if (!Has(app)) return wrapLog("not cached, won't update", null);
            var appState = Get(app);
            GetNewRepoLoader().Init(log).Update(appState, AppStateLoadSequence.ItemLoad, entities.ToArray(), log);
            return wrapLog("ok", appState);
        }

        #endregion




        /// <inheritdoc />
		public IAppIdentity GetIdentity(int? zoneId = null, int? appId = null) 
		{
			var resultZoneId = zoneId ?? (appId.HasValue
			                       ? Zones.Single(z => z.Value.Apps.Any(a => a.Key == appId.Value)).Key
			                       : Constants.DefaultZoneId);

			var resultAppId = appId.HasValue
								  ? Zones[resultZoneId].Apps.Single(a => a.Key == appId.Value).Key
								  : Zones[resultZoneId].DefaultAppId;

			return new AppIdentity(resultZoneId, resultAppId);
        }

    }
}
