using System;
using System.Runtime.CompilerServices;

namespace ToSic.Lib.Logging
{
    public partial interface ILog
    {
        //void Ex(Exception ex);

        /// <summary>
        /// Add a log entry for a class constructor, returning a method to call when done
        /// </summary>
        /// <param name="parameters">what was passed to the call in the brackets</param>
        /// <param name="message">the message to log</param>
        /// <param name="useTimer">enable a timer from call/close</param>
        /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
        /// <param name="cName">auto pre filled by the compiler - the method name</param>
        /// <param name="cLine">auto pre filled by the compiler - the code line</param>
        [Obsolete("Do not use any more, use Fn(...) extension method instead")]
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
        [Obsolete("Do not use any more, use Fn(...) extension method instead")]
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
        [Obsolete("Will remove soon - probably v15 as it's probably internal only")]
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
        [Obsolete("Will remove soon - probably v14 as it's probably internal only")]
        void Warn(string message);

        /// <summary>
        /// Add a message by calling a function. This will be inside a try/catch, to prevent crashes because of looping on nulls etc.
        /// </summary>
        /// <param name="messageMaker"></param>
        /// <param name="cPath">auto pre filled by the compiler - the path to the code file</param>
        /// <param name="cName">auto pre filled by the compiler - the method name</param>
        /// <param name="cLine">auto pre filled by the compiler - the code line</param>
        [Obsolete("Will remove soon - probably v14 as it's probably internal only")]
        void Add(Func<string> messageMaker,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0);

        [Obsolete("Will remove soon")]
        void Exception(Exception ex);
    }
}
