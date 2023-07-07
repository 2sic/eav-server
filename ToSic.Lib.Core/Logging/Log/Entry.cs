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
