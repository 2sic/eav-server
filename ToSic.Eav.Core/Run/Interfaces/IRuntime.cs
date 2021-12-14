using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Run
{
    public interface IRuntime: IHasLog<IRuntime>
    {

        List<IContentType> LoadGlobalContentTypes(int typeIdSeed);

        IEnumerable<Data.IEntity> LoadGlobalItems(string groupIdentifier);

    }
}
