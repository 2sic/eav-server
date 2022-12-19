//using System;
//using System.Runtime.CompilerServices;
//using ToSic.Eav.Data;
//using ToSic.Lib.Documentation;

//namespace ToSic.Eav.Logging
//{
//    /// <summary>
//    /// A logger with special capabilities.
//    /// It can take log messages, and chain itself to other loggers. <br/>
//    /// If chained, it can broadcast the messages to the other loggers from that time forward.
//    /// Basically this is the backbone of Insights.
//    /// </summary>
//    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
//    public interface ILog : IWrapper<Lib.Logging.ILog>
//    {
//        /*
//         * IMPORTANT
//         * ---------
//         * This interface must be copied 1:1 to ICodeLog
//         * Otherwise Razor won't be able to find these commands!
//         */


//        /// <summary>
//        /// Add a message log entry
//        /// </summary>
//        /// <param name="message">Message to log</param>
//        /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
//        /// <param name="cName">auto pre filled by the compiler - the method name</param>
//        /// <param name="cLine">auto pre filled by the compiler - the code line</param>
//        /// <returns>The same warning text which was added</returns>
//        [Obsolete("Will remove soon - probably v15 as it's probably internal only")]
//        string Add(string message,
//            [CallerFilePath] string cPath = null,
//            [CallerMemberName] string cName = null,
//            [CallerLineNumber] int cLine = 0
//        );

//        /// <summary>
//        /// Add a warning log entry
//        /// </summary>
//        /// <param name="message"></param>
//        /// <returns>The same warning text which was added</returns>
//        [Obsolete("Will remove soon - probably v14 as it's probably internal only")]
//        void Warn(string message);


//        [Obsolete("Will remove soon")]
//        void Exception(Exception ex);


//        /// <summary>
//        /// Add a log entry for method call, returning a method to call when done
//        /// </summary>
//        /// <param name="parameters">what was passed to the call in the brackets</param>
//        /// <param name="message">the message to log</param>
//        /// <param name="useTimer">enable a timer from call/close</param>
//        /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
//        /// <param name="cName">auto pre filled by the compiler - the method name</param>
//        /// <param name="cLine">auto pre filled by the compiler - the code line</param>
//        Action<string> Call(
//            string parameters = null, 
//            string message = null, 
//            bool useTimer = false,
//            [CallerFilePath] string cPath = null,
//            [CallerMemberName] string cName = null,
//            [CallerLineNumber] int cLine = 0
//        );

//        /// <summary>
//        /// Add a log entry for method call, returning a method to call when done
//        /// </summary>
//        /// <param name="parameters">what was passed to the call in the brackets</param>
//        /// <param name="message">the message to log</param>
//        /// <param name="useTimer">enable a timer from call/close</param>
//        /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
//        /// <param name="cName">auto pre filled by the compiler - the method name</param>
//        /// <param name="cLine">auto pre filled by the compiler - the code line</param>
//        /// <remarks>
//        /// Not used much, but major change in V15 - the first value in the result is the data, the second is the string to log.
//        /// Before it was (message, data), new is (data, message)
//        /// </remarks>
//        [Obsolete("Do not use any more, use Fn(...) extension method instead")]
//        Func<T, string, T> Call<T>(
//            string parameters = null, 
//            string message = null, 
//            bool useTimer = false,
//            [CallerFilePath] string cPath = null,
//            [CallerMemberName] string cName = null,
//            [CallerLineNumber] int cLine = 0
//        );
//    }
//}