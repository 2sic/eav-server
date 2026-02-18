namespace ToSic.Eav.DataSource;

/// <summary>
/// WIP interface to create one or many sources which can be attached when creating a new sources
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public interface IDataSourceLink : IDataSourceLinkable
{
    /// <summary>
    /// The data source being referenced.
    /// </summary>
    /// <remarks>
    /// This may be null if the data source is not available, but it would be a very extreme edge case.
    /// </remarks>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    IDataSource? DataSource { get; }

    /// <summary>
    /// The stream name on the out-connection.
    /// </summary>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    string OutName { get; }

    /// <summary>
    /// The stream name on the in-connection.
    /// </summary>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    string InName { get; }

    /// <summary>
    /// A directly defined stream to connect to.
    /// Takes precedence over Source if defined.
    /// </summary>
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
    /// Create a link with the same data source and stream, but different names. This is useful when you want to link the same data source multiple times with different names.
    /// Note that it is functional - if any name is different, it will create a new link, but if all names are the same, it will return the same link (as it's unmodified).
    /// </summary>
    /// <param name="outName">Rename the out-stream - rarely used since you would usually get the link from the correct Out by default</param>
    /// <param name="inName">Rename the in-stream</param>
    /// <returns></returns>
    IDataSourceLink WithRename(string? outName = default, string? inName = default);

    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    IDataSourceLink WithAnotherStream(string? name = default, string? outName = default, string? inName = default);

    /// <summary>
    /// Add one or more Links to this link for use when attaching to this and more sources in one step.
    /// </summary>
    /// <param name="more"></param>
    /// <returns></returns>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    IDataSourceLink WithMore(IDataSourceLinkable[] more);

}