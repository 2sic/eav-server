using System;
using System.Collections.Generic;
using ToSic.Eav.Logging.Simple;

//using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Logging
{
    public interface ILog
    {

        DateTime Created { get; }

        /// <summary>
        /// Add a log entry for method call, returning a method to call when done
        /// </summary>
        Action<string> Get(string property);

        Action<string> Set(string property);

        /// <summary>
        /// Intercept the result of an inner method, log it, then pass result on
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="generate"></param>
        /// <returns></returns>
        T Intercept<T>(string message, Func<T> generate);

        string Dump(string separator = " - ", string start = "", string end = "", string resultStart = "=>", string resultEnd = "");

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
        /// Add a message
        /// </summary>
        /// <param name="message"></param>
        string Add(string message);

        string Warn(string message);

        /// <summary>
        /// Add a message by calling a function. This will be inside a try/catch, to prevent crashes because of looping on nulls etc.
        /// </summary>
        /// <param name="messageMaker"></param>
        void Add(Func<string> messageMaker);

        List<Entry> Entries { get; }
        string FullIdentifier { get; }
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