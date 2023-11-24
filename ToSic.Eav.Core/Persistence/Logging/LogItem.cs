using System.Diagnostics;

namespace ToSic.Eav.Persistence.Logging;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LogItem
{
    public EventLogEntryType EntryType { get; }

    public string Message { get; private set; }

    public LogItem(EventLogEntryType entryType, string message)
    {
        EntryType = entryType;
        Message = message;
    }
}