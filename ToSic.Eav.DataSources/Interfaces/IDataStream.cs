using System;
using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Represents a DataStream object. This is a stream of IEntity objects, which has a source and a name.
	/// A stream can be read from, and it can be attached to upstream data-sources for further processing.
	/// </summary>
	[PublicApi_Stable_ForUseInYourCode]
	public interface IDataStream: ICanSelfCache, ICanPurgeListCache, IEnumerable<IEntity>
	{
        /// <summary>
        /// The list of items in this stream.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IEntity"/> items.</returns>
        IEnumerable<IEntity> List { get; }

        [PrivateApi]
        [Obsolete("deprecated since 2sxc 9.8 / eav 4.5 - use List instead")]
	    IEnumerable<IEntity> LightList { get; }

        /// <summary>
		/// Underlying <see cref="IDataSource"/> providing the <see cref="IEntity"/> of this stream
		/// </summary>
		/// <returns>The underlying <see cref="IDataSource"/></returns>
		IDataSource Source { get; }

		/// <summary>
		/// Name of this Stream
		/// </summary>
		/// <returns>The name - which would be used in the Source to get the same stream again.</returns>
		string Name { get; }

	}
}