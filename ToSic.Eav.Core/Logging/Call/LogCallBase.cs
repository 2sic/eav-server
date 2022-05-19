using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Logging.Call
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
            var entry = Entry = LogOrNull.AddInternal(openingMessage, code);
            entry.WrapOpen = true;
            LogOrNull.WrapDepth++;
            IsOpen = true;
        }

        public readonly Log LogOrNull;

        public Entry Entry { get; }

        public Stopwatch Stopwatch { get; }

        protected bool IsOpen;

        #region Basic Adds

        public void A(
            string message,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) => LogOrNull?.Add(message, cPath, cName, cLine);

        public void A(
            bool enabled, 
            string message,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        )
        {
            if (enabled) LogOrNull?.Add(message, cPath, cName, cLine);
        }


        #endregion

        public void DoInTimer(Action action)
        {
            var running = Stopwatch.IsRunning;
            if (!running) Stopwatch.Start();
            action();
            if (!running) Stopwatch.Stop();
        }

        public TResult DoInTimer<TResult>(Func<TResult> action)
        {
            var running = Stopwatch.IsRunning;
            if (!running) Stopwatch.Start();
            var result = action();
            if (!running) Stopwatch.Stop();
            return result;
        }



        protected void WrapFinish(Entry entry, string message, Stopwatch timer)
        {
            if (LogOrNull == null) return;

            LogOrNull.WrapDepth--;
            entry.AppendResult(message);
            var final = LogOrNull.AddInternal(null, null);
            final.WrapClose = true;
            final.AppendResult(message);
            if (timer == null) return;
            timer.Stop();
            entry.Elapsed = timer.Elapsed;
        }

        protected void DoneInternal(string message)
        {
            if (LogOrNull == null) return;

            if (!IsOpen)
                LogOrNull.AddInternal("Log Warning: Wrapper already closed from previous call", null);
            IsOpen = false;

            WrapFinish(Entry, message, Stopwatch);
        }

    }
}
