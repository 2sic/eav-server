using ToSic.Eav.Apps;
using ToSic.Eav.LookUp;
using ToSic.Eav.Work;

namespace ToSic.Eav.DataSource;

[ShowApiWhenReleased(ShowApiMode.Never)]
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

    /// <summary>
    /// WIP experimental v19.01 2dm
    /// </summary>
    IWorkSpecs Specs { get; }
}