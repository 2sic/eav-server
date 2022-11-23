//using System;
//using System.Diagnostics;

//namespace ToSic.Lib.Logging
//{
//    public partial class Log
//    {
//        /// <summary>
//        /// Wrap a log entry - basically log something with a prefix, 
//        /// and return a method which can be used for the done-message which will use the same prefix
//        /// </summary>
//        /// <param name="openingMessage"></param>
//        /// <param name="useTimer">optionally time the work done</param>
//        /// <returns></returns>
//        private Action<string> Wrapper(string openingMessage, bool useTimer, CodeRef code)
//        {
//            var entry = WrapStart(openingMessage, useTimer, code, out var timer);
//            return message => { WrapFinish(entry, message, timer); };
//        }

//        protected Entry WrapStart(string openingMessage, bool useTimer, CodeRef code, out Stopwatch timer)
//        {
//            var entry = this.AddInternalReuse(openingMessage, code);
//            entry.WrapOpen = true;
//            timer = useTimer ? Stopwatch.StartNew() : null;
//            WrapDepth++;
//            return entry;
//        }

//        protected void WrapFinish(Entry entry, string message, Stopwatch timer)
//        {
//            WrapDepth--;
//            entry.AppendResult(message);
//            var final = this.AddInternalReuse(null, null);
//            final.WrapClose = true;
//            final.AppendResult(message);
//            if (timer == null) return;
//            timer.Stop();
//            entry.Elapsed = timer.Elapsed;
//        }

//    }
//}
