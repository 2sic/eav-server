namespace ToSic.Eav.Logging.Simple
{
    public class Entry
    {
        public string Message { get; set; }

        private readonly Log _log;

        public string Source => _log.FullIdentifier;

        public Entry(Log log, string message)
        {
            _log = log;
            Message = message;
        }

        public string Serialize() => Message;
    }
}
