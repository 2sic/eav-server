using ToSic.Eav.Apps;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSource;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IDataSourceOptions
{
    IImmutableDictionary<string, string> Values { get; }

    /// <summary>
    /// The App Identity.
    /// If it is also a reader, it will be used for reading data.
    /// </summary>
    IAppIdentity AppIdentityOrReader { get; }

    ILookUpEngine LookUp { get; }

    bool? ShowDrafts { get; }

    bool Immutable { get; }
}