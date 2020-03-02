using System;
using System.Diagnostics;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log
    {
        /// <summary>
        /// Wrap a log entry - basically log something with a prefix, 
        /// and return a method which can be used for the done-message which will use the same prefix
        /// </summary>
        /// <param name="openingMessage"></param>
        /// <param name="useTimer">optionally time the work done</param>
        /// <returns></returns>
        private Action<string> Wrapper(string openingMessage, bool useTimer, CodeRef code)
        {
            var entry = WrapStart(openingMessage, useTimer, code, out var timer);
            return message => { WrapFinish(entry, message, timer); };
        }

        private Entry WrapStart(string openingMessage, bool useTimer, CodeRef code, out Stopwatch timer)
        {
            var entry = AddInternal(openingMessage, code);
            entry.WrapOpen = true;
            timer = useTimer ? Stopwatch.StartNew() : null;
            WrapDepth++;
            return entry;
        }

        private void WrapFinish(Entry entry, string message, Stopwatch timer)
        {
            WrapDepth--;
            entry.AppendResult(message);
            var final = AddInternal(null, null);
            final.WrapClose = true;
            final.AppendResult(message);
            if (timer == null) return;
            timer.Stop();
            entry.Elapsed = timer.Elapsed;
        }

        private Func<string, T, T> Wrapper<T>(string openingMessage, bool useTimer,
            CodeRef code)
        {
            var entry = WrapStart(openingMessage, useTimer, code, out var timer);
            var open = true;
            //var entry = AddInternal(openingMessage, code);
            //entry.WrapOpen = true;
            //var timer = useTimer ? Stopwatch.StartNew() : null;
            //WrapDepth++;
            return (message, result) =>
            {
                if (!open) 
                    Add("Log Warning: Wrapper already closed from previous call");
                open = false;

                WrapFinish(entry, message, timer);
                //WrapDepth--;
                //entry.AppendResult(message);
                //if (timer == null) return result;
                //timer.Stop();
                //entry.Elapsed = timer.Elapsed;
                return result;
            };
        }
    }
}
