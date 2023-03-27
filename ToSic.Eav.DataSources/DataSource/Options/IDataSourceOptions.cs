using System.Collections.Immutable;
using ToSic.Eav.Apps;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSource
{
    public interface IDataSourceOptions
    {
        IImmutableDictionary<string, string> Values { get; }

        IAppIdentity AppIdentity { get; }

        ILookUpEngine LookUp { get; }

        bool? ShowDrafts { get; }
    }
}
