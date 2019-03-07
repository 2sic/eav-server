using System;
using System.Collections.Generic;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Interfaces.Caches;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Represents a DataStream object. This is a stream of IEntity objects, which has a source and a name.
	/// A stream can be read from, and it can be attached to upstream data-sources for further processing.
	/// </summary>
	public interface IDataStream: ICanSelfCache, ICanPurgeListCache, IEnumerable<IEntity>
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