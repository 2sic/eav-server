using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Run.Unknown
{
    public class RuntimeUnknown: HasLog, IRuntime, IIsUnknown
    {
        public RuntimeUnknown(WarnUseOfUnknown<RuntimeUnknown> warn) : base("Eav.BscRnt") { }

        public IRuntime Init(ILog parent)
        {
            Log.LinkTo(parent);
            return this;
        }

        public AppState AppState() => new AppState(new ParentAppState(null, false), Constants.PresetIdentity, Constants.PresetName, new Log(LogNames.NotImplemented));

        public List<IContentType> LoadGlobalContentTypes(int seed) => new List<IContentType>();

        public IEnumerable<IEntity> LoadGlobalItems(string groupIdentifier) => new List<IEntity>();

    }
}
