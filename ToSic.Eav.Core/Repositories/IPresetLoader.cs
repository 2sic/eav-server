using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Logging;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Repositories
{
    public interface IPresetLoader: IHasLog<IPresetLoader>
    {
        /// <summary>
        /// Load the full AppState from the backend - maybe initialized, maybe not.
        /// </summary>
        /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
        /// <param name="ensureInitialized">if true, will enforce that app settings etc. are created</param>
        /// <remarks>
        /// Requesting Initialization may result in the app state being built twice - but that would only occur once per App ever (until it's initialized)
        /// </remarks>
        /// <returns>An object with everything which an app has, usually for caching</returns>
        AppState AppState(int appId);


        AppState Update(AppState app, AppStateLoadSequence startAt, int[] entityIds = null);

    }
}