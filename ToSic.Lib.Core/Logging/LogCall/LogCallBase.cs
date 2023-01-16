using System.Diagnostics;
using ToSic.Lib.Documentation;
using static System.String;

namespace ToSic.Lib.Logging
{
    [PrivateApi("no need to publish this")]
    public class LogCallBase : ILogLike, ILogCall
    {
        /// <summary>
        /// Keep constructor internal
        /// </summary>
        [PrivateApi]
        internal LogCallBase(ILog log,
            CodeRef code,
            bool isProperty,
            string parameters = null,
            string message = null,
            bool timer = false)
        {
            // Always init the stopwatch, as it could be used later even without a parent log
            Timer = timer ? Stopwatch.StartNew() : new Stopwatch();

            // Keep the log, but quit if it's not valid
            if (!(log.GetRealLog() is Log typedLog)) return;
            Log = typedLog;

            var openingMessage = $"{code.Name}" + (isProperty ? "" : $"({parameters})");
            if (!IsNullOrWhiteSpace(message)) 
                openingMessage += (IsNullOrWhiteSpace(openingMessage) ? "" : " ") + $"{message}";
            var entry = Entry = Log.AddInternalReuse(openingMessage, code);
            entry.WrapOpen = true;
            typedLog.WrapDepth++;
            //IsOpen = true;
        }

        /// <inheritdoc />
        public ILog Log { get; }

        /// <inheritdoc />
        public Entry Entry { get; }

        /// <inheritdoc />
        public Stopwatch Timer { get; }

        //[PrivateApi]
        //internal bool IsOpen;

        [PrivateApi("will probably remove")]
        public string NameId => Log?.NameId;

    }
}
