using System.Collections.Generic;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Implementations.Runtime
{
    public class NeutralRuntime: IRuntime
    {
        public IEnumerable<IContentType> LoadGlobalContentTypes() => new List<IContentType>();

        public IEnumerable<IEntity> LoadGlobalItems(string groupIdentifier) => new List<IEntity>();

        public ILog Log { get; } = new Log("empty");


        public void LinkLog(ILog parentLog) => Log.LinkTo(parentLog);
    }
}
