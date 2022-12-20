using System;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    [PrivateApi]
    internal static partial class LogExtensionsInternal
    {
        internal static ILog GetRealLog(this ILog log) => log as Log ?? (log as ILogLike)?.Log ?? log;


        /// <summary>
        /// Add a message
        /// </summary>
        internal static void AddInternal(this ILog log, string message, CodeRef code)
        {
            // Null-check
            if (!(log.GetRealLog() is ILogInternal realLog)) return;
            realLog.CreateAndAdd(message, code);
        }

        // todo: should become internal once ToSic.Eav.Logging isn't using it any more
        internal static Entry AddInternalReuse(this ILog log, string message, CodeRef code)
        {
            // Null-check
            if (!(log.GetRealLog() is ILogInternal realLog)) return new Entry(null, null, 0, code);
            var e = realLog.CreateAndAdd(message, code);
            return e;
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
