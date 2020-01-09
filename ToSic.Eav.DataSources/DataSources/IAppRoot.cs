using ToSic.Eav.Caching;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Interface for the App Root - usually the very first node in any data-delivery or query. <br/>
    /// It's just like a normal <see cref="IDataSource"/> but will internally access the <see cref="AppState"/> from the <see cref="IAppsCache"/>.
    /// </summary>
    [VisualQuery(GlobalName = "ToSic.Eav.DataSources.IAppRoot, ToSic.Eav.DataSources",
        NiceName = "App Root",
        PreviousNames = new []
            {
                "ToSic.Eav.DataSources.Caches.ICache, ToSic.Eav.DataSources",
                "ToSic.Eav.DataSources.Caching.IRootCache, ToSic.Eav.DataSources"
            },
        Type = DataSourceType.Source)]
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public interface IAppRoot : IDataSource
    {
        ///// <summary>
        ///// The AppState of the current app.
        ///// </summary>
        //AppState AppState { get; }

    }
}
