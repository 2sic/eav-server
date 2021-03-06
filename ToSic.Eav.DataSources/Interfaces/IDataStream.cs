﻿using System.Collections.Generic;
using System.Collections.Immutable;
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

		/// <summary>
		/// This is the real internal list, but the public one above "List" must be IEnumerable
		/// because otherwise Razor files would need to access the Immutable NuGets which is absurd
		/// </summary>
        [PrivateApi]
        IImmutableList<IEntity> Immutable { get; }

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



        /// <summary>
        /// Provide access to the CacheKey - so it could be overridden if necessary without using the stream underneath it
        /// </summary>
        [PrivateApi]
        DataStreamCacheStatus Caching { get; }
    }
}