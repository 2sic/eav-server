using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Logging
{
    public class HasLog : IHasLog
    {
        public string LogId { get; internal set; } = "unknwn";

        protected Log Log { get; private set; }


        public HasLog(string name, Log parentLog = null, string initialMessage = null)
        {
            InitLog(name, parentLog, initialMessage);
        }

        public void InitLog(string name, Log parentLog = null, string initialMessage = null)
        {
            if (Log == null)
                // standard & most common case: just create log
                Log = new Log(name, parentLog, initialMessage);
            else
            {
                // late-init case, where the log was already created - just reconfig keeping what was in it
                Log.Rename(name);
                LinkLog(parentLog);
                if (initialMessage != null)
                    Log.Add(initialMessage);
            }
        }

        public void LinkLog(Log parentLog) => Log.LinkTo(parentLog);
    }
}
