using ToSic.Eav.Logging;
using ToSic.Eav.Security.Fingerprint;

namespace ToSic.Eav.Configuration
{
    public class LoaderBase: HasLog
    {

        public LoaderBase(/*SystemFingerprint fingerprint,*/ LogHistory logHistory, ILog parentLog, string logName, string initialMessage) : base(logName, parentLog, initialMessage)
        {
            //Fingerprint = fingerprint;
            logHistory.Add(LogNames.LogHistoryGlobalTypes, Log);
        }
        //public IFingerprint Fingerprint { get; }
    }
}
