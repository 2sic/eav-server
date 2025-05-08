using System.Diagnostics;

namespace ToSic.Eav.Persistence.Logging;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LogItem(EventLogEntryType entryType, string message)
{
    public EventLogEntryType EntryType { get; } = entryType;

    public string Message { get; private set; } = message;
}