using System.Diagnostics;
using ToSic.Lib.Logging;
using Log = ToSic.Eav.Logging.Simple.Log;

namespace ToSic.Eav.Logging
{
    public class LogCallBase
    {
        /// <summary>
        /// Keep constructor internal
        /// </summary>
        internal LogCallBase(ILog log, 
            CodeRef code,
            bool isProp,
            string parameters = null,
            string message = null,
            bool startTimer = false)
        {
            // Always init the stopwatch, as it could be used later even without a parent log
            Stopwatch = startTimer ? Stopwatch.StartNew() : new Stopwatch();

            // Keep the log, but quit if it's not valid

            if (!(log is Log) && !(log is LogAdapter)) return;

            LogOrNull = log;
            var openingMessage = (isProp ? "." : "") + $"{code.Name}({parameters}) {message}";
            var entry = Entry = LogOrNull.AddInternalReuse(openingMessage, code);
            entry.WrapOpen = true; 
            if (log is Log simpleLog) simpleLog.WrapDepth++;
            if (log is LogAdapter logAdapter) logAdapter.WrapDepth++;
            IsOpen = true;
        }

        public readonly ILog LogOrNull;

        public Entry Entry { get; }

        public Stopwatch Stopwatch { get; }

        internal bool IsOpen;
        
    }
}
