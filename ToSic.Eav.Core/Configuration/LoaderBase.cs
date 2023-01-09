using ToSic.Lib.Logging;

namespace ToSic.Eav.Configuration
{
    public class LoaderBase: HasLog
    {
        public LoaderBase(ILogStore logStore, ILog parentLog, string logName, string initialMessage) : base(logName, parentLog, initialMessage)
        {
            logStore.Add(Lib.Logging.LogNames.LogStoreStartUp, Log);
        }
    }
}
