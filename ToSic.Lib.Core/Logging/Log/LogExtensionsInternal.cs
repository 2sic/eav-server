using System;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    [PrivateApi]
    internal static partial class LogExtensionsInternal
    {
        /// <summary>
        /// Get the real log object - either the object passed in, or if it's a LogCall, then the underlying log object.
        /// </summary>
        /// <param name="log"></param>
        /// <returns>The target Log object OR null</returns>
        internal static ILog GetRealLog(this ILog log) => log as Log ?? (log as ILogLike)?.Log ?? log;


        /// <summary>
        /// Add a message
        /// </summary>
        internal static Entry AddInternal(this ILog log, string message, CodeRef code)
        {
            // Null-check
            if (!(log.GetRealLog() is ILogInternal realLog)) return null;
            return realLog.CreateAndAdd(message, code);
        }

        internal static Entry AddInternalReuse(this ILog log, string message, CodeRef code)
        {
            return log.AddInternal(message, code) ?? new Entry(null, null, 0, code);
            //// Null-check
            //if (!(log.GetRealLog() is ILogInternal realLog)) return new Entry(null, null, 0, code);
            //var e = realLog.CreateAndAdd(message, code);
            //return e;
        }


        /// <summary>
        /// Try to call a function generating a message. 
        /// This will be inside a try/catch, to prevent crashes because of looping on nulls etc.
        /// </summary>
        /// <param name="messageMaker"></param>
        internal static string Try(Func<string> messageMaker)
        {
            try
            {
                return messageMaker.Invoke();
            }
            catch (Exception ex)
            {
                return "LOG: failed to generate from code, error: " + ex.Message;
            }
        }

    }
}
