using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Logging
{
    /// <summary>
    /// A logger with special capabilities.
    /// It can take log messages, and chain itself to other loggers. <br/>
    /// If chained, it can broadcast the messages to the other loggers from that time forward.
    /// Basically this is the backbone of Insights.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public interface ILog
    {
        /// <summary>
        /// When the log object was created - for rare output scenarios
        /// </summary>
        [PrivateApi]
        DateTime Created { get; }

        /// <summary>
        /// A short random ID to differentiate this logger from others.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// A unique identifier containing a special XXX.yyyyy[id] name
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// Intercept the result of an inner method, log it, then pass result on
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="generate"></param>
        /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
        /// <param name="cName">auto pre filled by the compiler - the method name</param>
        /// <param name="cLine">auto pre filled by the compiler - the code line</param>
        /// <returns></returns>
        [PrivateApi("not widely used yet, keep secret")] 
        T Intercept<T>(string message, Func<T> generate,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
            );

        /// <summary>
        /// Dump result to an internal format - not very important in public use cases
        /// </summary>
        [PrivateApi]
        string Dump(
            string separator = " - ",
            string start = "",
            string end = "",
            string resultStart = "=>",
            string resultEnd = "",
            bool withCaller = false,
            string callStart = "",
            string callEnd = ""
        );


        /// <summary>
        /// Add a log entry for method call, returning a method to call when done
        /// </summary>
        /// <param name="parameters">what was passed to the call in the brackets</param>
        /// <param name="message">the message to log</param>
        /// <param name="useTimer">enable a timer from call/close</param>
        /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
        /// <param name="cName">auto pre filled by the compiler - the method name</param>
        /// <param name="cLine">auto pre filled by the compiler - the code line</param>
        Action<string> Call(
            string parameters = null, 
            string message = null, 
            bool useTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        );

        /// <summary>
        /// Add a log entry for a class constructor, returning a method to call when done
        /// </summary>
        /// <param name="parameters">what was passed to the call in the brackets</param>
        /// <param name="message">the message to log</param>
        /// <param name="useTimer">enable a timer from call/close</param>
        /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
        /// <param name="cName">auto pre filled by the compiler - the method name</param>
        /// <param name="cLine">auto pre filled by the compiler - the code line</param>
        Action<string> Call(
            Func<string> parameters, 
            Func<string> message = null, 
            bool useTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        );


        /// <summary>
        /// Add a log entry for method call, returning a method to call when done
        /// </summary>
        /// <param name="parameters">what was passed to the call in the brackets</param>
        /// <param name="message">the message to log</param>
        /// <param name="useTimer">enable a timer from call/close</param>
        /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
        /// <param name="cName">auto pre filled by the compiler - the method name</param>
        /// <param name="cLine">auto pre filled by the compiler - the code line</param>
        Func<string, T, T> Call<T>(
            string parameters = null, 
            string message = null, 
            bool useTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        );


        /// <summary>
        /// Add a message log entry
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
        /// <param name="cName">auto pre filled by the compiler - the method name</param>
        /// <param name="cLine">auto pre filled by the compiler - the code line</param>
        /// <returns>The same warning text which was added</returns>
        string Add(string message,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
            );

        /// <summary>
        /// Add a warning log entry
        /// </summary>
        /// <param name="message"></param>
        /// <returns>The same warning text which was added</returns>
        string Warn(string message);

        /// <summary>
        /// Add a message by calling a function. This will be inside a try/catch, to prevent crashes because of looping on nulls etc.
        /// </summary>
        /// <param name="messageMaker"></param>
        /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
        /// <param name="cName">auto pre filled by the compiler - the method name</param>
        /// <param name="cLine">auto pre filled by the compiler - the code line</param>
        void Add(Func<string> messageMaker,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0);


        [PrivateApi]
        List<Entry> Entries { get; }

        [PrivateApi]
        string FullIdentifier { get; }

        /// <summary>
        /// Rename this logger - usually used when a base-class has a logger, 
        /// but the inherited class needs a different name
        /// </summary>
        /// <remarks>
        /// limits the length to 6 chars to make the output readable
        /// </remarks>
        /// <param name="name"></param>
        void Rename(string name);

        /// <summary>
        /// Link this logger to a parent
        /// and optionally rename
        /// </summary>
        /// <param name="parent">parent log to attach to</param>
        /// <param name="name">optional new name</param>
        void LinkTo(ILog parent, string name = null);
    }
}