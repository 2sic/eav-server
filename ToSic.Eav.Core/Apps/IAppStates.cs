using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    [PrivateApi("WIP")]
    public interface IAppStates
    {
        #region Get an App

        /// <summary>
        /// Retrieve an app from the cache
        /// </summary>
        /// <param name="app">App identifier.</param>
        /// <returns>The <see cref="AppState"/> of the app.</returns>
        AppState Get(IAppIdentity app);

        /// <summary>
        /// Retrieve an app from the cache
        /// </summary>
        /// <param name="appId">App id if zone unknown.</param>
        /// <returns>The <see cref="AppState"/> of the app.</returns>
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
}
