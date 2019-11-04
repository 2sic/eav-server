using System.Runtime.Serialization;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Logging
{
    public class HasLog : IHasLog
    {
        /// <summary>
        /// The unique ID of this logging item. <br/>
        /// It's usually a convention based name which helps identify logs of a specific class or object.
        /// The schema is Abc.AreaNm where the prefix marks a topic, the suffix the specific thing it's for. 
        /// </summary>
        [IgnoreDataMember]
        public string LogId { get; internal set; } = "unknwn";

        [IgnoreDataMember]
        public Log Log { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logName">Name to use in the Log-ID</param>
        /// <param name="parentLog">Parent log (if available) for log-chaining</param>
        /// <param name="initialMessage">First message to be added</param>
        /// <param name="className">Class name it's for</param>
        public HasLog(string logName, Log parentLog = null, string initialMessage = null, string className = null) 
            => InitLogInternal(logName, parentLog, initialMessage, className);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>this one can be overridden by outside sources, like the cache which should never allow attaching logs at runtime</remarks>
        /// <param name="name"></param>
        /// <param name="parentLog"></param>
        /// <param name="initialMessage"></param>
        public virtual void InitLog(string name, Log parentLog = null, string initialMessage = null) 
            => InitLogInternal(name, parentLog, initialMessage);

        private void InitLogInternal(string name, Log parentLog, string initialMessage, string className = null)
        {
            if (Log == null)
                // standard & most common case: just create log
                Log = new Log(name, parentLog, initialMessage);
            else
            {
                // late-init case, where the log was already created - just reconfig keeping what was in it
                Log.Rename(name);
                LinkLog(parentLog);
                if (initialMessage == null) return;
                if (className == null)
                    Log.Add(initialMessage);
                else
                    Log.New(className, initialMessage);
            }
        }

        public void LinkLog(Log parentLog) => Log.LinkTo(parentLog);
    }
}
