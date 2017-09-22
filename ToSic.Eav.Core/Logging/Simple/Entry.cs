namespace ToSic.Eav.Logging.Simple
{
    public class Entry
    {
        public string Message { get; set; }

        public Entry(string message)
        {
            Message = message;
        }

        public string Serialize() => Message;
    }
}
