using System.Diagnostics;

namespace ToSic.Lib.Logging
{
    public class LogCallBase : ILog

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
            if (!(log?._RealLog is Log typedLog)) return;
            _RealLog = typedLog;
            
            var openingMessage = (isProp ? "." : "") + $"{code.Name}({parameters}) {message}";
            var entry = Entry = _RealLog.AddInternalReuse(openingMessage, code);
            entry.WrapOpen = true;
            typedLog.WrapDepth++;
            IsOpen = true;
        }

        public ILog _RealLog { get; }

        public Entry Entry { get; }

        public Stopwatch Stopwatch { get; }

        internal bool IsOpen;


        public string NameId => _RealLog?.NameId;

        public bool Preserve
        {
            get => _RealLog?.Preserve ?? true;
            set
            {
                if (_RealLog != null) _RealLog.Preserve = value;
            }
        }


    }
}
