﻿using ToSic.Eav.DataSource.Sys.Streams;
using ToSic.Sys.Caching;


namespace ToSic.Eav.DataSource;

/// <summary>
/// Represents a DataStream object. This is a stream of IEntity objects, which has a source and a name.
/// A stream can be read from, and it can be attached to upstream data-sources for further processing.
/// </summary>
[PublicApi]
public interface IDataStream: ICanSelfCache, IEnumerable<IEntity>, IDataSourceLinkable
{
    /// <summary>
    /// The list of items in this stream.
    /// IMPORTANT: This is actually an Immutable List - so you can read it but not change it.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IEntity"/> items.</returns>
    IEnumerable<IEntity> List { get; }

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
    /// A special scope category for the stream.
    /// Important to hide system data types to the normal developer
    /// </summary>
    [PrivateApi("New / WIP 12.10")]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    string Scope { get; }

    /// <summary>
    /// Provide access to the CacheKey - so it could be overridden if necessary without using the stream underneath it
    /// </summary>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    DataStreamCacheStatus Caching { get; }

    /// <summary>
    /// Reset the stream - this will clear the local and force a reload of the data.
    /// It will not flush the up-stream caches.
    /// </summary>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    void ResetStream();
}