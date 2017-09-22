namespace ToSic.Eav.Logging.Simple
{
    public class Entry
    {
        public string Source { get; set; }
        public string Message { get; set; }

        public Entry(string source, string message)
        {
            Source = source;
            Message = message;
        }

        public string Serialize() => Message;
    }
}
