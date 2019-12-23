using ToSic.Eav.Documentation;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// The App Root is the entry point for all data. It takes it's data from a hidden AppState Cache.
    /// It's implemented as a DataSource so that other DataSources can easily attach to it. <br/>
    /// This is also the object returned as the root in any query.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public class AppRoot : DataSourceBase, IAppRoot
    {
        [PrivateApi]
        public override string LogId => "DS.Root";

        [PrivateApi]
        public AppRoot()
        {
            //Root = this;

		    // ReSharper disable VirtualMemberCallInConstructor
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, () => AppState.List));
			Out.Add(Constants.PublishedStreamName, new DataStream(this, Constants.PublishedStreamName, () => AppState.ListPublished));
			Out.Add(Constants.DraftsStreamName, new DataStream(this, Constants.DraftsStreamName, () => AppState.ListNotHavingDrafts));
		    // ReSharper restore VirtualMemberCallInConstructor
		}

	    /// <summary>
		/// Gets the KeySchema used to store values for a specific Zone and App. Must contain {0} for ZoneId and {1} for AppId
		/// </summary>
		[PrivateApi]
        private static string CacheKeySchema => "Z{0}A{1}";


        /// <summary>
        /// Get the <see cref="AppState"/> of this app from the cache.
        /// </summary>
	    private AppState AppState => _appState ?? (_appState = Factory.GetAppState(this));
        private AppState _appState;

        #region Cache-Chain

        /// <inheritdoc />
        public override long CacheTimestamp => AppState.CacheTimestamp;

	    /// <inheritdoc />
	    public override bool CacheChanged(long newCacheTimeStamp) => AppState.CacheChanged(newCacheTimeStamp);

	    /// <inheritdoc />
	    public override string CachePartialKey => _cachePartialKey ?? (_cachePartialKey = string.Format(CacheKeySchema, ZoneId, AppId));
        private string _cachePartialKey;

	    /// <inheritdoc />
	    public override string CacheFullKey => CachePartialKey;

        #endregion

    }
}
