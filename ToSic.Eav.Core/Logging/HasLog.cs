using System.Runtime.Serialization;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Logging
{
    public class HasLog : IHasLog
    {
        [IgnoreDataMember]
        public string LogId { get; internal set; } = "unknwn";

        [IgnoreDataMember]
        public Log Log { get; private set; }


        public HasLog(string logName, Log parentLog = null, string initialMessage = null)
        {
            InitLogInternal(logName, parentLog, initialMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>this one can be overridden by outside sources, like the cache which should never allow attaching logs at runtime</remarks>
        /// <param name="name"></param>
        /// <param name="parentLog"></param>
        /// <param name="initialMessage"></param>
        public virtual void InitLog(string name, Log parentLog = null, string initialMessage = null) 
            => InitLogInternal(name, parentLog, initialMessage);

        private void InitLogInternal(string name, Log parentLog, string initialMessage)
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
