using System.Collections.Generic;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.Apps.Interfaces
{
    public interface IApp : IAppIdentity
    {
        string Name { get; }
        string Folder { get; }
        bool Hidden { get; }
        string AppGuid { get; }


        IAppData Data { get; }
        IDictionary<string, IDataSource> Query { get; }

    }
}
