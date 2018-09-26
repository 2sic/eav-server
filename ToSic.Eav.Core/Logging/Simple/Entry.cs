namespace ToSic.Eav.Logging.Simple
{
    public class Entry
    {
        public string Message { get; }
        public string Result { get; private set; }
        public int Depth;

        private readonly Log _log;

        public string Source => _log.FullIdentifier;

        public Entry(Log log, string message, int depth)
        {
            _log = log;
            Message = message;
            Depth = depth;
        }

        public void AppendResult(string message) => Result = message;
    }
}
