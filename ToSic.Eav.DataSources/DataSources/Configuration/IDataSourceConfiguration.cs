using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSources
{
    public interface IDataSourceConfiguration
    {
        IDictionary<string, string> Values { get; }

        IAppIdentity AppIdentity { get; }

        ILookUpEngine LookUp { get; }

        bool? ShowDrafts { get; }
    }
}
