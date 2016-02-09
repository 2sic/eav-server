﻿using System.Collections.Generic;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Represents a DataStream
	/// </summary>
	public interface IDataStream
	{
		/// <summary>
		/// Dictionary of Entites in this Stream
		/// </summary>
		IDictionary<int, IEntity> List { get; }
        IEnumerable<IEntity> LightList { get; }
        
        /// <summary>
		/// DataSource providing the Entities
		/// </summary>
		IDataSource Source { get; }
		/// <summary>
		/// Name of this Stream
		/// </summary>
		string Name { get; }


        #region Self-Caching and Results-Persistence Properties / Features
        /// <summary>
        /// This one will return the original result if queried again - as long as this object exists
        /// </summary>
        bool KeepResultsAfterFirstQuery { get; set; }

        /// <summary>
        /// Place the stream in the cache if wanted, by default not
        /// </summary>
        bool AutoCaching { get; set; }

        /// <summary>
        /// Default cache duration is 3600
        /// </summary>
        int CacheDurationInSeconds { get; set; }

        /// <summary>
        /// Kill the cache if the source data is newer than the cache-stamped data
        /// </summary>
        bool CacheRefreshOnSourceRefresh { get; set; }

        #endregion
	}
}