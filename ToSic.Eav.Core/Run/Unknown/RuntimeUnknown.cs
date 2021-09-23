using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
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

        public IEnumerable<IContentType> LoadGlobalContentTypes() => new List<IContentType>();

        public IEnumerable<IEntity> LoadGlobalItems(string groupIdentifier) => new List<IEntity>();

    }
}
