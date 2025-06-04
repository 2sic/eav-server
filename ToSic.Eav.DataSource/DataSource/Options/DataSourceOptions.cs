using ToSic.Eav.Apps;
using ToSic.Lib.LookUp.Engines;
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
    public IImmutableDictionary<string, string> Values { get; init; }

    // todo: improve to probably be IAppIdentity or IAppReader
    public IAppIdentity AppIdentityOrReader { get; init; }
    public ILookUpEngine? LookUp { get; init; }
    public bool? ShowDrafts { get; init; }
    public bool Immutable { get; init; }

    public IWorkSpecs Specs { get; init; }
}