﻿

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources;

/// <summary>
/// Represents a data source that can be the recipient of Data.
/// This basically means it has an In <see cref="IDataStream"/>
/// </summary>
[PrivateApi]
[Obsolete("Obsolete since v15.04 - will be removed ca. v17")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IDataTarget
{
    // Removed from IDataTarget in 15.06
    ///// <summary>
    ///// List of all In connections
    ///// </summary>
    //IDictionary<string, IDataStream> In { get; }
		
    /// <summary>
    /// Attach a DataSource to In - replaces all existing in-streams.
    /// </summary>
    /// <param name="dataSource">DataSource to attach</param>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    void Attach(IDataSource dataSource);

    /// <summary>
    /// Add a single named stream to the In
    /// </summary>
    /// <param name="streamName">In-name of the stream</param>
    /// <param name="dataSource">The data source - will use it's default out</param>
    /// <param name="sourceName">The stream name on the source, will default to "Default"</param>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    void Attach(string streamName, IDataSource dataSource, string sourceName = DataSourceConstants.StreamDefaultName);

    /// <summary>
    /// Add a single named stream to the In
    /// </summary>
    /// <param name="streamName">In-name of the stream</param>
    /// <param name="dataStream">The data stream to attach</param>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    void Attach(string streamName, IDataStream dataStream);
}