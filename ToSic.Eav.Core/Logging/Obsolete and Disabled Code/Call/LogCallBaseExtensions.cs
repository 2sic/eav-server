//using System;
//using System.Runtime.CompilerServices;

//namespace ToSic.Eav.Logging
//{
//    public static class LogCallBaseExtensions
//    {
//        #region A = Add

//        public static void A(this LogCallBase logCall,
//            string message,
//            [CallerFilePath] string cPath = null,
//            [CallerMemberName] string cName = null,
//            [CallerLineNumber] int cLine = 0
//        ) => logCall?.LogOrNull?.A(message, cPath, cName, cLine);


//        public static void A(this LogCallBase logCall,
//            bool enabled,
//            string message,
//            [CallerFilePath] string cPath = null,
//            [CallerMemberName] string cName = null,
//            [CallerLineNumber] int cLine = 0
//        ) => logCall?.LogOrNull?.A(enabled, message, cPath, cName, cLine);

//        #endregion

//        #region DoInTimer

//        public static void DoInTimer(this LogCallBase logCall, Action action)
//        {
//            var skipOwnTimer = logCall?.Stopwatch.IsRunning ?? true;
//            if (!skipOwnTimer) logCall.Stopwatch.Start();
//            action();
//            if (!skipOwnTimer) logCall.Stopwatch.Stop();
//        }

//        public static TResult DoInTimer<TResult>(this LogCallBase logCall, Func<TResult> action)
//        {
//            var skipOwnTimer = logCall?.Stopwatch.IsRunning ?? true;
//            if (!skipOwnTimer) logCall.Stopwatch.Start();
//            var result = action();
//            if (!skipOwnTimer) logCall.Stopwatch.Stop();
//            return result;
//        }


//        #endregion


//        internal static void DoneInternal(this LogCallBase logCall, string message)
//        {
//            if (logCall?.LogOrNull == null) return;
//            var log = logCall.LogOrNull;

//            if (!logCall.IsOpen)
//                log.AddInternal("Log Warning: Wrapper already closed from previous call", null);
//            logCall.IsOpen = false;

//            //log.WrapDepth--;
//            if (log is Simple.Log simpleLog) simpleLog.WrapDepth--;
//            if (log is LogAdapter logAdapter) logAdapter.WrapDepth--;

//            logCall.Entry.AppendResult(message);
//            var final = log.AddInternalReuse(null, null);
//            final.WrapClose = true;
//            final.AppendResult(message);
//            if (logCall.Stopwatch == null) return;
//            logCall.Stopwatch.Stop();
//            logCall.Entry.Elapsed = logCall.Stopwatch.Elapsed;
//        }

//    }
//}
