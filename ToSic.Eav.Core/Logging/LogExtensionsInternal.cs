using System;
using ToSic.Eav.Logging.Simple;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Logging
{
    internal static partial class LogExtensionsInternal
    {

        /// <summary>
        /// Add a message
        /// </summary>
        internal static void AddInternal(this ILog log, string message, CodeRef code)
        {
            // Null-check
            if (!(log is Log realLog)) return;
            var e = new Entry(log, message, realLog.WrapDepth, code);
            log.AddToEntriesAndParent(e);
        }

        internal static Entry AddInternalReuse(this ILog log, string message, CodeRef code)
        {
            // Null-check
            if (!(log is Log realLog)) return new Entry(null, null, 0, code);
            var e = new Entry(log, message, realLog.WrapDepth, code);
            log.AddToEntriesAndParent(e);
            return e;
        }

        /// <summary>
        /// add an existing entry of another logger
        /// </summary>
        /// <param name="log"></param>
        /// <param name="entry"></param>
        internal static void AddToEntriesAndParent(this ILog log, Entry entry)
        {
            // prevent parallel threads from updating entries at the same time
            lock (log.Entries) { log.Entries.Add(entry); }
            (log.Parent as Log)?.AddToEntriesAndParent(entry);
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
