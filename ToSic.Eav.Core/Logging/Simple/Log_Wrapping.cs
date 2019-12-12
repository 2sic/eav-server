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
        private Action<string> Wrapper(string openingMessage, bool useTimer)
        {
            var entry = AddEntry(openingMessage);
            var timer = useTimer ? Stopwatch.StartNew() : null;
            WrapDepth++;
            return message =>
            {
                WrapDepth--;
                entry.AppendResult(message);
                if (timer == null) return;
                timer.Stop();
                entry.Elapsed = timer.Elapsed;
            };
        }

        private Func<string, T, T> Wrapper<T>(string openingMessage, bool useTimer)
        {
            var entry = AddEntry(openingMessage);
            var timer = useTimer ? Stopwatch.StartNew() : null;
            WrapDepth++;
            return (message, result) =>
            {
                WrapDepth--;
                entry.AppendResult(message);
                if (timer == null) return result;
                timer.Stop();
                entry.Elapsed = timer.Elapsed;
                return result;
            };
        }
    }
}
