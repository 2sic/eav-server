using System.Diagnostics;

namespace ToSic.Lib.Logging
{
    public class LogCallBase : ILog, ILogLike

    {
        /// <summary>
        /// Keep constructor internal
        /// </summary>
        internal LogCallBase(ILog log,
            CodeRef code,
            bool isProperty,
            string parameters = null,
            string message = null,
            bool startTimer = false)
        {
            // Always init the stopwatch, as it could be used later even without a parent log
            Stopwatch = startTimer ? Stopwatch.StartNew() : new Stopwatch();

            // Keep the log, but quit if it's not valid
            if (!(log is Log typedLog)) return;
            Log = typedLog;
            
            var openingMessage = $"{code.Name}" + (isProperty ? "" : $"({parameters})") + $" {message}";
            var entry = Entry = Log.AddInternalReuse(openingMessage, code);
            entry.WrapOpen = true;
            typedLog.WrapDepth++;
            IsOpen = true;
        }

        public ILog Log { get; }

        public Entry Entry { get; }

        public Stopwatch Stopwatch { get; }

        internal bool IsOpen;


        public string NameId => Log?.NameId;

    }
}
