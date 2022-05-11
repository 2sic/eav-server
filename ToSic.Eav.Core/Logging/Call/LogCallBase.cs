using System;
using System.Diagnostics;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Logging.Call
{
    public class LogCallBase
    {

        internal LogCallBase(ILog log, 
            CodeRef code,
            bool isProp,
            string parameters = null,
            string message = null,
            bool startTimer = false)
        {
            Log = log as Log;
            var openingMessage = (isProp ? ".": "") + $"{code.Name}({parameters}) {message}";
            var entry = Entry = Log.AddInternal(openingMessage, code);
            entry.WrapOpen = true;
            Stopwatch = startTimer ? Stopwatch.StartNew() : new Stopwatch();
            Log.WrapDepth++;
            IsOpen = true;
        }

        public readonly Log Log;

        public Entry Entry { get; }

        public Stopwatch Stopwatch { get; }

        protected bool IsOpen;

        public void DoInTimer(Action action)
        {
            var timer = Stopwatch;
            var running = timer.IsRunning;
            if (!running) timer.Start();
            action();
            if (!running) timer.Stop();
        }

        public TResult DoInTimer<TResult>(Func<TResult> action)
        {
            var timer = Stopwatch;
            var running = timer.IsRunning;
            if (!running) timer.Start();
            var result = action();
            if (!running) timer.Stop();
            return result;
        }



        protected void WrapFinish(Entry entry, string message, Stopwatch timer)
        {
            Log.WrapDepth--;
            entry.AppendResult(message);
            var final = Log.AddInternal(null, null);
            final.WrapClose = true;
            final.AppendResult(message);
            if (timer == null) return;
            timer.Stop();
            entry.Elapsed = timer.Elapsed;
        }

        protected void DoneInternal(string message)
        {
            if (!IsOpen)
                Log.Add("Log Warning: Wrapper already closed from previous call");
            IsOpen = false;

            WrapFinish(Entry, message, Stopwatch);
        }

    }
}
