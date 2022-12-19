﻿using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// Base class for most objects which simply want to implement log and log-chaining.
    /// </summary>
    //[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public abstract class HasLog : IHasLog
    {
        /// <inheritdoc />
        [JsonIgnore]
        [IgnoreDataMember]
        public ILog Log { get; }

        /// <summary>
        /// Constructor which ensures Log-chaining and optionally adds initial messages
        /// </summary>
        /// <param name="logName">Name to use in the Log-ID</param>
        /// <param name="parentLog">Parent log (if available) for log-chaining</param>
        /// <param name="initialMessage">First message to be added</param>
        /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
        /// <param name="cName">auto pre filled by the compiler - the method name</param>
        /// <param name="cLine">auto pre filled by the compiler - the code line</param>
        protected HasLog(string logName, ILog parentLog = null, string initialMessage = null, 
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)

            : this(logName, CodeRef.Create(cPath, cName, cLine), parentLog, initialMessage) {}

        protected HasLog(string logName, CodeRef code, ILog parentLog = null, string initialMessage = null)
        {
            Log = new Log(logName, parentLog, code, initialMessage);
            //InitLog(logName, parentLog, initialMessage, code);
        }

        ///// <summary>
        ///// This is the real initializer - implemented as a virtual method, because some
        ///// long-living objects must actively prevent logs from being attached. 
        ///// </summary>
        ///// <remarks>this one can be overridden by outside sources, like the cache which should never allow attaching logs at runtime</remarks>
        ///// <param name="name"></param>
        ///// <param name="parentLog"></param>
        ///// <param name="initialMessage"></param>
        //// TODO: @2dm - probably review again, used to be virtual, not sure if this should be done this way or another
        //// Seems to only be used on data-sources to init their log...
        //public void InitLog(string name, ILog parentLog = null, string initialMessage = null) 
        //    => InitLog(name, parentLog, initialMessage, null);



        //private void InitLog(string name,
        //    ILog parentLog, 
        //    string initialMessage, 
        //    CodeRef code
        //    )
        //{
        //    //if (Log == null)
        //        // standard & most common case: just create log
        //        Log = new Log(name, parentLog, code, initialMessage);
        //    //else
        //    //{
        //    //    // late-init case, where the log was already created - just reconfigure keeping what was in it
        //    //    Log.Rename(name);
        //    //    this.Init(parentLog);
        //    //    if (initialMessage == null) return;
        //    //    Log.A(initialMessage, code?.Path, code?.Name, code?.Line ?? 0);
        //    //}
        //}

    }
}
