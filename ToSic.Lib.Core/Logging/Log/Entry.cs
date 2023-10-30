using System;

namespace ToSic.Lib.Logging
{
    //[PrivateApi]
    public class Entry
    {
        public string Message { get; }
        public string Result { get; private set; }
        public TimeSpan Elapsed { get; set; }
        public int Depth;

        public bool WrapOpen;
        public bool WrapClose;
        public bool WrapOpenWasClosed;

        public readonly EntryOptions Options;

        private readonly ILog _log;

        public string Source => (_log as Log)?.FullIdentifier;

        public string ShortSource => _log.NameId;

        internal Entry(ILog log, string message, int depth, CodeRef code, EntryOptions options = default)
        {
            _log = log;
            Message = message;
            Depth = depth;
            Options = options;
            Code = code;
#if DEBUG
            // Do depth check, as < 0 should never be possible
            // but only on debug, as we would rather ignore this during production since the effect is minimal
            // it's more of an indication that some log wraps incorrectly
            if (depth < 0)
            {
                // attach debugger here
#pragma warning disable CS0219
                var x = 0;
#pragma warning restore CS0219
            }
#endif
        }

        public void AppendResult(string message)
        {
            Result = message;
            WrapOpenWasClosed = true;
        }

        #region Stack Information

        /// <summary>
        /// Code reference where the log was added
        /// </summary>
        public CodeRef Code;

        #endregion
    }
}
