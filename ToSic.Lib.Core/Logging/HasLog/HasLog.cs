//using System.Runtime.CompilerServices;
//using System.Runtime.Serialization;
//using System.Text.Json.Serialization;

//namespace ToSic.Lib.Logging
//{
//    /// <summary>
//    /// Base class for most objects which simply want to implement log and log-chaining.
//    /// </summary>
//    //[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
//    public abstract class HasLog : IHasLog
//    {
//        /// <inheritdoc />
//        [JsonIgnore]
//        [IgnoreDataMember]
//        public ILog Log { get; }

//        /// <summary>
//        /// Constructor for most basic use case with just the log name.
//        /// </summary>
//        /// <param name="logName">Name to use in the Log-ID</param>
//        protected HasLog(string logName) : this(logName, CodeRef.Create("", "", default)) {}

//        ///// <summary>
//        ///// Constructor which ensures Log-chaining and optionally adds initial messages
//        ///// </summary>
//        ///// <param name="logName">Name to use in the Log-ID</param>
//        ///// <param name="parentLog">Parent log (if available) for log-chaining</param>
//        //protected HasLog(string logName, ILog parentLog)
//        //    : this(logName, CodeRef.Create("", "", default), parentLog, default) {}

//        ///// <summary>
//        ///// Constructor which ensures Log-chaining and optionally adds initial messages
//        ///// </summary>
//        ///// <param name="logName">Name to use in the Log-ID</param>
//        ///// <param name="parentLog">Parent log (if available) for log-chaining</param>
//        ///// <param name="initialMessage">First message to be added</param>
//        ///// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
//        ///// <param name="cName">auto pre filled by the compiler - the method name</param>
//        ///// <param name="cLine">auto pre filled by the compiler - the code line</param>
//        //protected HasLog(string logName, ILog parentLog = default, string initialMessage = default, 
//        //    [CallerFilePath] string cPath = default,
//        //    [CallerMemberName] string cName = default,
//        //    [CallerLineNumber] int cLine = default)
//        //    : this(logName, CodeRef.Create(cPath, cName, cLine), parentLog, initialMessage) {}

//        /// <summary>
//        /// Rarely used constructor with already created code-ref
//        /// </summary>
//        protected HasLog(string logName, CodeRef code, ILog parentLog = default, string initialMessage = default) 
//            => Log = new Log(logName, parentLog, code, initialMessage);
//    }
//}
