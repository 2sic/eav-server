using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Configuration
{
    public class LoaderBase: ServiceBase
    {
        public LoaderBase(ILogStore logStore, string logName) : base(logName)
        {
            logStore.Add(Lib.Logging.LogNames.LogStoreStartUp, Log);
        }
    }
}
