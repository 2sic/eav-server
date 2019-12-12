using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Logging.Simple
{
    [PrivateApi]
    public class Entry
    {
        public string Message { get; }
        public string Result { get; private set; }
        public TimeSpan Elapsed { get; set; }
        public int Depth;

        private readonly ILog _log;

        public string Source => _log.FullIdentifier;

        public Entry(ILog log, string message, int depth, string callerPath = "", string callerName = "", int callerLine = 0)
        {
            _log = log;
            Message = message;
            Depth = depth;
            CallerPath = callerPath;
            CallerName = callerName;
            CallerLine = callerLine;
        }

        public void AppendResult(string message) => Result = message;

        #region Stack Information (experimental)

        public string CallerPath;
        public string CallerName;
        public int CallerLine;

        #endregion
    }
}
