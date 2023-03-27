using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Caching;
using ToSic.Eav.DataSources.Caching;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
using static ToSic.Eav.DataSource.DataSourceConstants;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// The App Root is the entry point for all data. It takes it's data from a hidden AppState Cache.
    /// It's implemented as a DataSource so that other DataSources can easily attach to it. <br/>
    /// This is also the object returned as the root in any query.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public class AppRoot : Eav.DataSource.DataSourceBase, IAppRoot
    {
        [PrivateApi]
        public AppRoot(IAppStates appStates, MyServices services) : base(services, $"{LogPrefix}.Root")
        {
            _appStates = appStates;
            ProvideOut(() => AppState.List);
            ProvideOut(() => AppState.ListPublished.List, StreamPublishedName);
            ProvideOut(() => AppState.ListNotHavingDrafts.List, StreamDraftsName);
		}
        private readonly IAppStates _appStates;

        IDataSourceLink IDataSourceLinkable.Links => _link.Get(() => new DataSourceLink(null, dataSource: this)
            .Add(new DataSourceLink(null, dataSource: this, name: StreamPublishedName),
                new DataSourceLink(null, dataSource: this, name: StreamDraftsName)));
        private readonly GetOnce<IDataSourceLink> _link = new GetOnce<IDataSourceLink>();

        /// <summary>
        /// Special CacheKey generator for AppRoots, which rely on the state
        /// </summary>
        [PrivateApi]
        public new ICacheKeyManager CacheKey => _cacheKey ?? (_cacheKey = new AppRootCacheKey(this));
        private CacheKey _cacheKey;

        /// <summary>
        /// Get the <see cref="AppState"/> of this app from the cache.
        /// </summary>
	    private AppState AppState => _appState ?? (_appState = _appStates.Get(this));
        private AppState _appState;

        #region Cache-Chain

        /// <inheritdoc />
        public override long CacheTimestamp => AppState.CacheTimestamp;

	    /// <inheritdoc />
	    public override bool CacheChanged(long dependentTimeStamp) => AppState.CacheChanged(dependentTimeStamp);

	    /// <inheritdoc />
	    public override string CacheFullKey => CachePartialKey;

        #endregion

    }
}
