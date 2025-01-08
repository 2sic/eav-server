using ToSic.Eav.Apps;
using ToSic.Eav.LookUp;
using ToSic.Eav.Work;

namespace ToSic.Eav.DataSource;

/// <summary>
/// Provide setup configuration for a new data source.
///
/// This is internal functionality
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed record DataSourceOptions: IDataSourceOptions
{
    public IImmutableDictionary<string, string> Values { get; init; }

    // todo: improve to probably be IAppIdentity or IAppReader
    public IAppIdentity AppIdentityOrReader { get; init; }
    public ILookUpEngine LookUp { get; init; }
    public bool? ShowDrafts { get; init; }
    public bool Immutable { get; init; }

    public IWorkSpecs Specs { get; init; }
}