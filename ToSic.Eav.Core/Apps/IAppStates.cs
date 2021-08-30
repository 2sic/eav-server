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

        /// <summary>
        /// Get/Resolve ZoneId and AppId for specified ZoneId and/or AppId. If both are null, default ZoneId with it's default App is returned.
        /// </summary>
        /// <returns>An <see cref="IAppIdentity"/> with the correct IDs</returns>
        IAppIdentity Identity(int? zoneId = null, int? appId = null);

        #endregion

    }
}
