using System;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging.Simple
{
    [PrivateApi]
    public class Entry
    {
        public string Message { get; }
        public string Result { get; private set; }
        public TimeSpan Elapsed { get; set; }
        public int Depth;

        public bool WrapOpen;
        public bool WrapClose;
        public bool WrapOpenWasClosed;

        private readonly ILog _log;

        public string Source => _log.FullIdentifier;

        public string ShortSource => _log.Identifier;

        public Entry(ILog log, string message, int depth, CodeRef code)
        {
            _log = log;
            Message = message;
            Depth = depth;
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
