using System.Diagnostics;
using ToSic.Eav.Logging.Simple;

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
        

        //protected void WrapFinish(Entry entry, string message)
        //{
        //    if (LogOrNull == null) return;

        //    LogOrNull.WrapDepth--;
        //    entry.AppendResult(message);
        //    var final = LogOrNull.AddInternalReuse(null, null);
        //    final.WrapClose = true;
        //    final.AppendResult(message);
        //    if (Stopwatch == null) return;
        //    Stopwatch.Stop();
        //    entry.Elapsed = Stopwatch.Elapsed;
        //}

        //internal void DoneInternal(string message)
        //{
        //    if (LogOrNull == null) return;

        //    if (!IsOpen)
        //        LogOrNull.AddInternal("Log Warning: Wrapper already closed from previous call", null);
        //    IsOpen = false;

        //    this.WrapFinish(Entry, message);
        //}

    }
}
