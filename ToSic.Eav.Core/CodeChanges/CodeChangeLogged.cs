using ToSic.Lib.Logging;

namespace ToSic.Eav.CodeChanges
{
    public class CodeChangeLogged
    {
        public CodeChangeLogged(CodeChangeUse use, LogStoreEntry entry = default)
        {
            Use = use;
            EntryOrNull = entry;
        }

        public CodeChangeUse Use { get; }
        public LogStoreEntry EntryOrNull { get; }
    }
}
