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

}