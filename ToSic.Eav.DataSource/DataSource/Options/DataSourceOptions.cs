using ToSic.Eav.Apps;
using ToSic.Eav.LookUp.Sys.Engines;
using ToSic.Sys.Work;

namespace ToSic.Eav.DataSource;

/// <summary>
/// Provide setup configuration for a new data source.
///
/// This is internal functionality
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record DataSourceOptions: IDataSourceOptions
{
    public DataSourceOptions() { }

    public IImmutableDictionary<string, string>? MyConfigValues { get; init; }

    // todo: improve to probably be IAppIdentity or IAppReader
    // #WipAppIdentityOrReader must become not null
    public required IAppIdentity? AppIdentityOrReader { get; init; }
    public ILookUpEngine? LookUp { get; init; }
    public bool? ShowDrafts { get; init; }
    public bool Immutable { get; init; }

    public IWorkSpecs? Specs { get; init; }

    public IDataSourceLinkable? Attach { get; init; }

    /// <summary>
    /// WIP to keep track of all creations where the AppIdentityOrReader is not yet set, but should be. This will be used to find all places where this needs to be fixed.
    /// </summary>
    /// <returns></returns>
    public static DataSourceOptions Empty() => new()
    {
        AppIdentityOrReader = null, // #WipAppIdentityOrReader must become not null
    };

    public static DataSourceOptions OfDataSource(IDataSource source) => new()
    {
        AppIdentityOrReader = source,
        Attach = source,
    };
}