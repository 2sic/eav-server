using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.DataSources.Caching.CacheInfo;
using ToSic.Eav.DataSources.Configuration;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Public interface for an Eav DataSource. All DataSource objects are based on this. 
	/// </summary>
	[PublicApi_Stable_ForUseInYourCode]
	public interface IDataSource : IAppIdentity, ICacheInfo, ICanPurgeListCache, IHasLog
	{
		#region Data Interfaces

        /// <summary>
        /// Internal ID usually from persisted configurations IF the configuration was build from an pre-stored query.
        /// </summary>
        /// <returns>The guid of this data source which identifies the configuration <see cref="IEntity"/> of the data source.</returns>
        Guid Guid { get; set; }

		/// <summary>
		/// Gets the Dictionary of Out-Streams. This is the internal accessor, as usually you'll use this["name"] instead. <br/>
		/// In rare cases you need the Out, for example to list the stream names in the data source.
		/// </summary>
		/// <returns>A dictionary of named <see cref="IDataStream"/> objects</returns>
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
        IImmutableList<IEntity> List { get; }

        [PrivateApi("wip")]
        DataSourceConfiguration Configuration { get; }

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

	    #region Internals 

		/// <summary>
		/// Name of this DataSource - not usually relevant.
		/// </summary>
		/// <returns>Name of this source.</returns>
		string Name { get; }
		#endregion

        #region Caching Information

        /// <summary>
        /// Some configuration of the data source is cache-relevant, others are not.
        /// This list contains the names of all configuration items which are cache relevant.
        /// It will be used when generating a unique ID for caching the data.
        /// </summary>
        List<string> CacheRelevantConfigurations { get; set; }

        ICacheKeyManager CacheKey { get; }

        /// <summary>
        /// Tell the system that out is dynamic and doesn't have a fixed list of streams.
        /// Used by App-Data sources and similar.
        /// Important for the global information system, so it doesn't try to query that. 
        /// </summary>
        [PrivateApi]
        bool OutIsDynamic { get; }
        #endregion
    }

}
