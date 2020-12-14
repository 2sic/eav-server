using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Logging;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Repositories
{
    public interface IRepositoryLoader: IHasLog<IRepositoryLoader>, IContentTypeLoader
    {
        /// <summary>
        /// Get EntityModel for specified EntityId
        /// </summary>
        /// <returns>A single IEntity or throws InvalidOperationException</returns>
        /// <summary>Get Data to populate ICache</summary>
        /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
        /// <param name="parentLog">parent logger</param>
        /// <returns>An object with everything which an app has, usually for caching</returns>
        AppState AppState(int appId, ILog parentLog = null);

        /// <summary>
        /// Get EntityModel for specified EntityId
        /// </summary>
        /// <returns>A single IEntity or throws InvalidOperationException</returns>
        /// <summary>Get Data to populate ICache</summary>
        /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
        /// <param name="ensureInitialized">if true, will enforce that app settings etc. are created</param>
        /// <param name="parentLog">parent logger</param>
        /// <returns>An object with everything which an app has, usually for caching</returns>
        AppState AppState(int appId, bool ensureInitialized, ILog parentLog = null);


        AppState Update(AppState app, AppStateLoadSequence startAt, int[] entityIds = null, ILog parentLog = null);


        IReadOnlyDictionary<int, Zone> Zones();

        string PrimaryLanguage { get; set; }
    }
}