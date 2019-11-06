using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Interfaces
{
    public interface IRuntime: IHasLog
    {
        IEnumerable<IContentType> LoadGlobalContentTypes();

        IEnumerable<IEntity> LoadGlobalItems(string groupIdentifier);

    }
}
