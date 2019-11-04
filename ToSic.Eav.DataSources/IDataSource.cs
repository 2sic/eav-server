﻿using System;
using System.Collections.Generic;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Documentation;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Interfaces.Caches;
using ToSic.Eav.ValueProvider;
using ICache = ToSic.Eav.DataSources.Caches.ICache;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Public interface for an Eav DataSource. All DataSource objects are based on this. 
	/// </summary>
	[PublicApi]
	public interface IDataSource : IAppIdentity, ICacheExpiring, ICacheKeyProvider, ICanPurgeListCache
	{
		#region Data Interfaces

        /// <summary>
        /// Internal ID usually from persisted configurations IF the configuration was build from an pre-stored query.
        /// </summary>
        /// <returns>The guid of this data source which identifies the configuration <see cref="IEntity"/> of the data source.</returns>
        Guid DataSourceGuid { get; set; }

		/// <summary>
		/// Gets the Dictionary of Out-Streams. This is the internal accessor, as usually you'll use this["name"] instead.
		/// </summary>
		[PrivateApi]
        IDictionary<string, IDataStream> Out { get; }

		/// <summary>
		/// Gets the Out-Stream with specified Name. 
		/// </summary>
		/// <returns>an <see cref="IDataStream"/> of the desired name</returns>
		/// <exception cref="NullReferenceException">if the stream does not exist</exception>
		IDataStream this[string outName] { get; }

        /// <summary>
        /// The items in the data-source - to be exact, the ones in the Default stream.
        /// </summary>
        /// <returns>A list of <see cref="IEntity"/> items in the Default stream.</returns>
        IEnumerable<IEntity> List { get; }

        [PrivateApi]
        [Obsolete("deprecated since 2sxc 9.8 / eav 4.5 - use List instead")]
        IEnumerable<IEntity> LightList { get; }

        /// <summary>
		/// Gets the ConfigurationProvider for this DataSource
		/// </summary>
        IValueCollectionProvider ConfigurationProvider { get; }

		/// <summary>
		/// Gets a Dictionary of Configurations for this DataSource, e.g. Key: EntityId, Value: [QueryString:EntityId]
		/// </summary>
		IDictionary<string, string> Configuration { get; }

        #endregion

        #region UI Interfaces -- not implemented yet

        ///// <summary>
        ///// if the UI should show editing features for the user
        ///// </summary>
        //bool AllowUserEdit { get; }
        ///// <summary>
        ///// if the UI should show sorting features for the user
        ///// </summary>
        //bool AllowUserSort { get; }

        ///// <summary>
        ///// if the UI should show versioning features for the user
        ///// </summary>
        //bool AllowVersioningUI { get; }

	    #endregion

	    #region Internals (Ready, DistanceFromSource)
	    /// <summary>
	    /// Indicates whether the DataSource is ready for use (initialized/configured)
		/// </summary>
        bool Ready { get; }

		/// <summary>
		/// Name of this DataSource - not usually relevant.
		/// </summary>
		/// <returns>Name of this source.</returns>
		string Name { get; }
		#endregion

        #region Caching Information

        /// <summary>
        /// TODO
        /// </summary>
        [PrivateApi]
	    ICache Cache { get; }

        /// <summary>
        /// List of items from the configuration which should be used for creating the cache-key
        /// </summary>
        [PrivateApi]
        List<string> CacheRelevantConfigurations { get; set; }

        [PrivateApi]
        bool TempUsesDynamicOut { get; }
        #endregion
    }

}
