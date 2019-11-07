using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Interfaces
{
    public interface IRuntime: IHasLog
    {
        IEnumerable<IContentType> LoadGlobalContentTypes();

        IEnumerable<Data.IEntity> LoadGlobalItems(string groupIdentifier);

    }
}
