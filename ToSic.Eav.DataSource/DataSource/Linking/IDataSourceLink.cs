﻿namespace ToSic.Eav.DataSource;

/// <summary>
/// WIP interface to create one or many sources which can be attached when creating a new sources
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public interface IDataSourceLink : IDataSourceLinkable
{
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    IDataSource? DataSource { get; }
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    string OutName { get; }
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    string InName { get; }
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    IDataStream? Stream { get; }

    /// <summary>
    /// Internal use only.
    /// </summary>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    IEnumerable<IDataSourceLink> More { get; }

    /// <summary>
    /// Rename aspects of the current link.
    /// </summary>
    /// <param name="name">If provided, will rename both out and in</param>
    /// <param name="outName">Rename the out-stream - rarely used since you would usually get the link from the correct Out by default</param>
    /// <param name="inName">Rename the in-stream</param>
    /// <returns></returns>
    IDataSourceLink Rename(string? name = default, string? outName = default, string? inName = default);

    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    IDataSourceLink AddStream(string? name = default, string? outName = default, string? inName = default);

    /// <summary>
    /// Add one or more Links to this link for use when attaching to this and more sources in one step.
    /// </summary>
    /// <param name="more"></param>
    /// <returns></returns>
    IDataSourceLink Add(params IDataSourceLinkable[] more);

    /// <summary>
    /// Internal use only - flatten all the links in this object.
    /// </summary>
    /// <param name="recursion"></param>
    /// <returns></returns>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    IEnumerable<IDataSourceLink> Flatten(int recursion = 0);
}