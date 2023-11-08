using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ToSic.Eav.Apps;
using ToSic.Lib.Logging;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Repositories
{
    public interface IRepositoryLoader: IHasLog, IContentTypeLoader
    {
        ///// <summary>
        ///// Load the full AppState from the backend - maybe initialized, maybe not.
        ///// </summary>
        ///// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
        ///// <param name="ensureInitialized">if true, will enforce that app settings etc. are created</param>
        ///// <remarks>
        ///// Requesting Initialization may result in the app state being built twice - but that would only occur once per App ever (until it's initialized)
        ///// </remarks>
        ///// <returns>An object with everything which an app has, usually for caching</returns>
        //AppState AppState(int appId, bool ensureInitialized);


        /// <summary>
        /// Special loader which won't initialize the state.
        /// We're creating an own API for this, to better track down where things come from in case something is surprisingly wrong.
        /// </summary>
        /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
        /// <param name="cPath">Auto added origin, for debugging in case of trouble</param>
        /// <param name="cName">Auto added origin, for debugging in case of trouble</param>
        /// <param name="cLine">Auto added origin, for debugging in case of trouble</param>
        /// <returns></returns>
        AppState AppStateRaw(int appId, [CallerFilePath] string cPath = default, [CallerMemberName] string cName = default, [CallerLineNumber] int cLine = default);

        /// <summary>
        /// will enforce that app settings etc. are created
        /// </summary>
        /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
        /// <param name="cPath">Auto added origin, for debugging in case of trouble</param>
        /// <param name="cName">Auto added origin, for debugging in case of trouble</param>
        /// <param name="cLine">Auto added origin, for debugging in case of trouble</param>
        /// <returns></returns>
        AppState AppStateInitialized(int appId, [CallerFilePath] string cPath = default, [CallerMemberName] string cName = default, [CallerLineNumber] int cLine = default);

        AppState Update(AppState app, AppStateLoadSequence startAt, int[] entityIds = null);


        IDictionary<int, Zone> Zones();

        string PrimaryLanguage { get; set; }
    }
}