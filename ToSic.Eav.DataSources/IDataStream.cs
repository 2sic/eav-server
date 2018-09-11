using System;
using System.Collections.Generic;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Interfaces.Caches;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Represents a DataStream
	/// </summary>
	public interface IDataStream: ICanSelfCache, ICanPurgeListCache
	{
        IEnumerable<IEntity> List { get; }

        [Obsolete("deprecated since 2sxc 9.8 / eav 4.5 - use List instead")]
	    IEnumerable<IEntity> LightList { get; }

        /// <summary>
		/// DataSource providing the Entities
		/// </summary>
		IDataSource Source { get; }
		/// <summary>
		/// Name of this Stream
		/// </summary>
		string Name { get; }

	}
}