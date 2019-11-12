using System.Runtime.Serialization;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Logging
{
    /// <summary>
    /// Base class for most objects which simply want to implement log and log-chaining.
    /// </summary>
    [PublicApi]
    public abstract class HasLog : IHasLog
    {
        ///// <summary>
        ///// The unique ID of this logging item. <br/>
        ///// It's usually a convention based name which helps identify logs of a specific class or object.
        ///// The schema is Abc.AreaNm where the prefix marks a topic, the suffix the specific thing it's for. 
        ///// </summary>
        //[IgnoreDataMember]
        //[PrivateApi] // I think this is not in use?
        //public string LogId { get; internal set; } = "unknwn";

        /// <inheritdoc />
        [IgnoreDataMember]
        public ILog Log { get; private set; }

        /// <summary>
        /// Constructor which ensures Log-chaining and optionally adds initial messages
        /// </summary>
        /// <param name="logName">Name to use in the Log-ID</param>
        /// <param name="parentLog">Parent log (if available) for log-chaining</param>
        /// <param name="initialMessage">First message to be added</param>
        /// <param name="className">Class name it's for</param>
        protected HasLog(string logName, ILog parentLog = null, string initialMessage = null, string className = null) 
            => InitLogInternal(logName, parentLog, initialMessage, className);

        /// <summary>
        /// This is the real initializer - implemented as a virtual method, because some
        /// long-living objects must actively prevent logs from being attached. 
        /// </summary>
        /// <remarks>this one can be overridden by outside sources, like the cache which should never allow attaching logs at runtime</remarks>
        /// <param name="name"></param>
        /// <param name="parentLog"></param>
        /// <param name="initialMessage"></param>
        public virtual void InitLog(string name, ILog parentLog = null, string initialMessage = null) 
            => InitLogInternal(name, parentLog, initialMessage);

        private void InitLogInternal(string name, ILog parentLog, string initialMessage, string className = null)
        {
            if (Log == null)
                // standard & most common case: just create log
                Log = new Log(name, parentLog, initialMessage);
            else
            {
                // late-init case, where the log was already created - just reconfig keeping what was in it
                Log.Rename(name);
                this.LinkLog(parentLog);
                if (initialMessage == null) return;
                if (className == null)
                    Log.Add(initialMessage);
                else
                    Log.New(className, initialMessage);
            }
        }

    }
}
