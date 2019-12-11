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
    [PublicApi]
    public interface ILog
    {
        /// <summary>
        /// When the log object was created - for rare output scenarios
        /// </summary>
        [PrivateApi]
        DateTime Created { get; }

        /// <summary>
        /// Add a special log entry for a Getter, returning a method to call when the get completes
        /// </summary>
        Action<string> Get(string property);

        /// <summary>
        /// Add a special log entry for a Setter, returning a method to call when the get completes
        /// </summary>
        Action<string> Set(string property);

        /// <summary>
        /// Intercept the result of an inner method, log it, then pass result on
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="generate"></param>
        /// <returns></returns>
        [PrivateApi] // not widely used yet, keep secret
        T Intercept<T>(string message, Func<T> generate);

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
        /// Add a log entry for a class constructor, returning a method to call when done
        /// </summary>
        Action<string> New(string className, string @params = null, string message = null);

        /// <summary>
        /// Add a log entry for a class constructor, returning a method to call when done
        /// </summary>
        Action<string> New(string className, Func<string> @params, Func<string> message = null);

        /// <summary>
        /// Add a log entry for method call, returning a method to call when done
        /// </summary>
        Action<string> Call(string methodName, string @params = null, string message = null);

        /// <summary>
        /// Add a log entry for a class constructor, returning a method to call when done
        /// </summary>
        Action<string> Call(string methodName, Func<string> @params, Func<string> message = null);

        /// <summary>
        /// Add a log entry for method call, returning a method to call when done
        /// </summary>
        Func<string, T, T> Call<T>(string methodName, string @params = null, string message = null);

        /// <summary>
        /// Add a message log entry
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="callerPath">auto pre filled by the compiler - the path to the code file</param>
        /// <param name="callerName">auto pre filled by the compiler - the method name</param>
        /// <param name="callerLine">auto pre filled by the compiler - the code line</param>
        /// <returns>The same warning text which was added</returns>
        string Add(string message,
            [CallerFilePath] string callerPath = "",
            [CallerMemberName] string callerName = "",
            [CallerLineNumber] int callerLine = 0);

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
        void Add(Func<string> messageMaker);


        [PrivateApi]
        List<Entry> Entries { get; }

        [PrivateApi]
        string FullIdentifier { get; }

        [PrivateApi]
        ILog AddChild(string name, string message);

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