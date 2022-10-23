using System.Diagnostics;
using ToSic.Eav.Logging.Simple;
using ToSic.Lib.Logging;

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
            LogOrNull = log as Log;
            if (LogOrNull == null) return;

            var openingMessage = (isProp ? ".": "") + $"{code.Name}({parameters}) {message}";
            var entry = Entry = LogOrNull.AddInternalReuse(openingMessage, code);
            entry.WrapOpen = true;
            LogOrNull.WrapDepth++;
            IsOpen = true;
        }

        public readonly Log LogOrNull;

        public Entry Entry { get; }

        public Stopwatch Stopwatch { get; }

        internal bool IsOpen;
        
    }
}
