using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Caching
{
    /// <summary>
    /// Marks the objects which are responsible for caching <see cref="AppState"/> in memory. <br/>
    /// This is a very powerful system ensuring performance and more. 
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public interface IAppsCache
    {
        #region Get an App

        /// <summary>
        /// Retrieve an app from the cache
        /// </summary>
        /// <param name="sp">Current service provider, in case the app must be retrieved / generated</param>
        /// <param name="app">App identifier.</param>
        /// <returns>The <see cref="AppState"/> of the app.</returns>
        AppState Get(IServiceProvider sp, IAppIdentity app);

        #endregion

        #region Zones

        ///// <summary>
        ///// The list of zones, which internally contains the list of apps. 
        ///// </summary>
        [PrivateApi]
        IReadOnlyDictionary<int, Zone> Zones(IServiceProvider sp);

        [PrivateApi]
        int ZoneIdOfApp(IServiceProvider sp, int appId);

        #endregion

        #region inspect cache

        /// <summary>
        /// Check if something is already in the cache
        /// </summary>
        /// <param name="app">App identifier.</param>
        /// <returns></returns>
        bool Has(IAppIdentity app);

        #endregion

        #region Cache Purging

        /// <summary>
        /// Clean cache for specific Zone and App
        /// </summary>
        void Purge(IAppIdentity app);


        /// <summary>
        /// Clean entire global cache, which includes the List of Zones and Apps as well as all the apps.
        /// </summary>
        void PurgeZones();

        #endregion

        #region partial updates

        /// <summary>
        /// Tell the cache that an app has done a partial update. Only relevant for farm scenarios, where other nodes must be informed.
        /// </summary>
        /// <param name="app">App identifier.</param>
        /// <param name="entities">List of entities which need to be updates.</param>
        /// <param name="log">Log object to log what's happening.</param>
        /// <returns>The updated <see cref="AppState"/> or null, if it wasn't in the cache so didn't need updating.</returns>
        AppState Update(IServiceProvider sp, IAppIdentity app, IEnumerable<int> entities, ILog log);

        [PrivateApi("wip 12.10+")]
        void Add(AppState appState);

        #endregion

        #region PreLoading of Cache when the primary language needs to be specified

        /// <summary>
        /// Load an app into cache, specifying the primary language.
        /// This is used in scenarios, where the primary language cannot be auto-detected, so it's set explicitly.
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="app">App identifier.</param>
        /// <param name="primaryLanguage">Primary language, lower case.</param>
        void Load(IServiceProvider sp, IAppIdentity app, string primaryLanguage);

        #endregion

    }
}
