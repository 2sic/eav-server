using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Interface for the App Root - usually the very first node in any data-delivery or query. 
    /// </summary>
    [VisualQuery(GlobalName = "ToSic.Eav.DataSources.IAppRoot, ToSic.Eav.DataSources",
        NiceName = "App Root",
        PreviousNames = new []
            {
                "ToSic.Eav.DataSources.Caches.ICache, ToSic.Eav.DataSources",
                "ToSic.Eav.DataSources.Caching.IRootCache, ToSic.Eav.DataSources"
            },
        Type = DataSourceType.Source)]
    [PublicApi]
    public interface IAppRoot : IDataSource
    {

        #region Content Type Stuff - probably we should remove this from the RootCache

        /// <summary>
        /// Gets a ContentType by Name
        /// </summary>
        [PrivateApi("probably deprecate, as you should only use the AppState and actually create an AppState, not get it from the root cache?")]
		IContentType GetContentType(string name);


        #endregion


        /// <summary>
        /// Retrieve the AppState of the current app.
        /// </summary>
        AppState AppState { get; }


    }
}
