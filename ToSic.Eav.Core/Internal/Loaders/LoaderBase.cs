using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Internal.Loaders
{
    public class LoaderBase: ServiceBase
    {
        public LoaderBase(ILogStore logStore, string logName) : base(logName)
        {
            logStore.Add(LogNames.LogStoreStartUp, Log);
        }
    }
}
