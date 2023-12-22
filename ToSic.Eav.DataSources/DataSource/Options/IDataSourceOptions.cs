using System.Collections.Immutable;
using ToSic.Eav.Apps;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSource;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IDataSourceOptions
{
    IImmutableDictionary<string, string> Values { get; }

    IAppIdentity AppIdentity { get; }

    ILookUpEngine LookUp { get; }

    bool? ShowDrafts { get; }

    bool Immutable { get; }
}