using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Implementations.Runtime
{
    public class NeutralRuntime:IRuntime
    {
        public IEnumerable<IContentType> LoadGlobalContentTypes() => new List<IContentType>();

        public IEnumerable<IEntity> LoadGlobalItems(string groupIdentifier) => new List<IEntity>();
    }
}
