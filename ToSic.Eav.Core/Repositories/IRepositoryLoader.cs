using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Logging;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Repositories
{
    public interface IRepositoryLoader: IHasLog<IRepositoryLoader>, IContentTypeLoader
    {
        /// <summary>
        /// Load the full AppState from the backend - in an un-initialized state (without folder / name etc.).
        /// This is mostly for internal operations where initialization would cause trouble or unexpected side-effects.
        /// </summary>
        /// <returns>A single IEntity or throws InvalidOperationException</returns>
        /// <summary>Get Data to populate ICache</summary>
        /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
        /// <param name="parentLog">parent logger</param>
        /// <returns>An object with everything which an app has, usually for caching</returns>
        AppState AppState(int appId, ILog parentLog = null);

        /// <summary>
        /// Load the full AppState from the backend - maybe initialized, maybe not.
        /// </summary>
        /// <returns>A single IEntity or throws InvalidOperationException</returns>
        /// <summary>Get Data to populate ICache</summary>
        /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
        /// <param name="ensureInitialized">if true, will enforce that app settings etc. are created</param>
        /// <param name="parentLog">parent logger</param>
        /// <remarks>
        /// Requesting Initialization may result in the app state being built twice - but that would only occur once per App ever (until it's initialized)
        /// </remarks>
        /// <returns>An object with everything which an app has, usually for caching</returns>
        AppState AppState(int appId, bool ensureInitialized, ILog parentLog = null);


        AppState Update(AppState app, AppStateLoadSequence startAt, int[] entityIds = null, ILog parentLog = null);


        IReadOnlyDictionary<int, Zone> Zones();

        string PrimaryLanguage { get; set; }
    }
}