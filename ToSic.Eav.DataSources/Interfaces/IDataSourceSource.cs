using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Eav.Conventions;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.DataSources.Caching.CacheInfo;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Public interface for an Eav DataSource. All DataSource objects are based on this. 
	/// </summary>
	[PublicApi_Stable_ForUseInYourCode]
	public interface IDataSourceSource: IDataSourceShared, IAppIdentity, ICacheInfo, ICanPurgeListCache, IHasLog, IGetAccessors<string>, ISetAccessorsGeneric
    {
		#region Data Interfaces

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
        /// Gets the Out-Stream with specified Name and allowing some error handling if not found.
        /// </summary>
        /// <param name="name">The desired stream name. If empty, will default to the default stream.</param>
        /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        /// <param name="nullIfNotFound">In case the stream `name` isn't found, will return null. Ideal for chaining with ??</param>
        /// <param name="emptyIfNotFound">In case the stream `name` isn't found, will return an empty stream. Ideal for using LINQ directly.</param>
        /// <returns>an <see cref="IDataStream"/> of the desired name</returns>
        /// <exception cref="NullReferenceException">if the stream does not exist and `nullIfNotFound` is false</exception>
        /// <remarks>
        /// 1. Added in 2sxc 12.05
        /// 1. for more in-depth checking if a stream exists, you can access the <see cref="Out"/> which is an IDictionary
        /// </remarks>
        IDataStream GetStream(string name = null, string noParamOrder = Parameters.Protector, bool nullIfNotFound = false, bool emptyIfNotFound = false);

        /// <summary>
        /// The items in the data-source - to be exact, the ones in the Default stream.
        /// </summary>
        /// <returns>A list of <see cref="IEntity"/> items in the Default stream.</returns>
        IEnumerable<IEntity> List { get; }

        /// <summary>
        /// The configuration system of this data source.
        /// Keeps track of all values which the data source will need, and manages the LookUp engine
        /// which provides these values. 
        /// </summary>
        IDataSourceConfiguration Configuration { get; }

        #endregion

        #region Caching Information

        /// <summary>
        /// Some configuration of the data source is cache-relevant, others are not.
        /// This list contains the names of all configuration items which are cache relevant.
        /// It will be used when generating a unique ID for caching the data.
        /// </summary>
        List<string> CacheRelevantConfigurations { get; set; }

        ICacheKeyManager CacheKey { get; }

        #endregion

        #region Error Handler

        /// <summary>
        /// Special helper to generate error-streams.
        ///
        /// DataSources should never `throw` exceptions but instead return a stream containing the error information.
        /// </summary>
        DataSourceErrorHelper Error { get; }


        #endregion
    }

}
