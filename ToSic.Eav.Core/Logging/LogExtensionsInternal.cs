//using System;
//using System.Collections.Generic;
//using ToSic.Lib.Logging;
//using Log = ToSic.Eav.Logging.Simple.Log;

//namespace ToSic.Eav.Logging
//{
//    internal static partial class LogExtensionsInternal
//    {

//        /// <summary>
//        /// Add a message
//        /// </summary>
//        internal static void AddInternal(this ILog log, string message, CodeRef code)
//        {
//            // Null-check
//            if (!(log is Log) && !(log is LogAdapter)) return;
//            var e = new Entry(log, message, GetWrapDepth(log), code);
//            log.AddToEntriesAndParent(e);
//        }

//        internal static Entry AddInternalReuse(this ILog log, string message, CodeRef code)
//        {
//            // Null-check
//            if (!(log is Log) && !(log is LogAdapter)) return new Entry(null, null, 0, code);
//            var e = new Entry(log, message, GetWrapDepth(log), code);
//            log.AddToEntriesAndParent(e);
//            return e;
//        }

//        private static int GetWrapDepth(ILog log)
//        {
//            switch (log)
//            {
//                case Log simpleLog:
//                    return simpleLog.WrapDepth;
//                case LogAdapter logAdapter:
//                    return logAdapter.WrapDepth;
//                default:
//                    return 0;
//            }
//        }

//        /// <summary>
//        /// add an existing entry of another logger
//        /// </summary>
//        /// <param name="log"></param>
//        /// <param name="entry"></param>
//        internal static void AddToEntriesAndParent(this ILog log, Entry entry)
//        {
//            if (!(log is Log) && !(log is LogAdapter)) return;
//            var entries = GetEntries(log);
//            // prevent parallel threads from updating entries at the same time
//            lock (entries) { entries.Add(entry); }

//            if (log is Log simpleLog) simpleLog.Parent?.AddToEntriesAndParent(entry);
//            if (log is LogAdapter logAdapter) logAdapter.L.Parent?.AddToEntriesAndParent(entry);
//        }

//        private static List<Entry> GetEntries(ILog log)
//        {
//            switch (log)
//            {
//                case Log simpleLog:
//                    return simpleLog.Entries;
//                case LogAdapter logAdapter:
//                    return logAdapter.L.Entries;
//                default:
//                    return null; //new List<Entry>();
//            }
//        }

//        /// <summary>
//        /// Try to call a function generating a message. 
//        /// This will be inside a try/catch, to prevent crashes because of looping on nulls etc.
//        /// </summary>
//        /// <param name="messageMaker"></param>
//        internal static string Try(Func<string> messageMaker)
//        {
//            try
//            {
//                return messageMaker.Invoke();
//            }
//            catch (Exception ex)
//            {
//                return "LOG: failed to generate from code, error: " + ex.Message;
//            }
//        }

//    }
//}
