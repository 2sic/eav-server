using ToSic.Lib.Logging;

namespace ToSic.Eav.Configuration
{
    public class LoaderBase: HasLog
    {
        public LoaderBase(History logHistory, ILog parentLog, string logName, string initialMessage) : base(logName, parentLog, initialMessage)
        {
            logHistory.Add(LogNames.LogHistoryGlobalAndStartUp, Log);
        }
    }
}
