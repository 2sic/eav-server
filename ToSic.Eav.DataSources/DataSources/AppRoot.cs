﻿using System.Collections.Generic;
using ToSic.Eav.Caching.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using AppState = ToSic.Eav.Apps.AppState;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// The App Root is the entry point for all data. It takes it's data from a hidden AppState Cache.
    /// It's implemented as a DataSource so that other DataSources can easily attach to it. <br/>
    /// This is also the object returned as the root in any query.
    /// </summary>
    [PublicApi]
    public class AppRoot : DataSourceBase, IMetadataSource, IAppRoot
    {
        [PrivateApi] 
        public IAppsCache AppsCache => _appsCache ?? (_appsCache = Factory.Resolve<IAppsCache>());
        private IAppsCache _appsCache;

        [PrivateApi]
        public override string LogId => "DS.Root";

        [PrivateApi]
        public AppRoot()
        {
            Root = this;

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
        private string CacheKeySchema => "Z{0}A{1}";


        /// <summary>
        /// Get the <see cref="AppState"/> of this app from the cache.
        /// </summary>
	    public AppState AppState => _appState ?? (_appState =  AppsCache.Get(this));

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

        #region ContentType - probably remove from RootCache soon

        /// <inheritdoc />
        /// <summary>
        /// Get a ContentType by StaticName if found of DisplayName if not
        /// </summary>
        /// <param name="name">Either StaticName or DisplayName</param>
        /// <returns>a content-type OR null</returns>
        [PrivateApi("probably deprecate, as you should only use the AppState and actually create an AppState, not get it from the root cache?")]
	    public IContentType GetContentType(string name) => AppState.GetContentType(name);

        #endregion

        /// <summary>
        /// Get all Content Types
        /// </summary>
        [PrivateApi("probably deprecate, as you should only use the AppState?")]
		public IEnumerable<IContentType> GetContentTypes() => AppState.ContentTypes;


        #region GetAssignedEntities by Guid, string and int

	    /// <inheritdoc />
        public IEnumerable<IEntity> Get<T>(int targetType, T key, string contentTypeName = null) 
            => AppState.Get(targetType, key, contentTypeName);

	    #endregion

    }
}
