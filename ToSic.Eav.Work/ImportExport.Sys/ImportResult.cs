using ToSic.Eav.Persistence.Sys.Logging;

namespace ToSic.Eav.ImportExport.Sys;

public class ImportResult
{
    public bool Success { get; set; }

    public List<Message> Messages { get; set; } = [];

    /// <summary>
    /// Empty constructor for deserialization.
    /// </summary>
    public ImportResult() {}

    /// <summary>
    /// Quick create with optional message.
    /// </summary>
    public ImportResult(bool success = false, string? msg = null, Message.MessageTypes type = Message.MessageTypes.Information)
    {
        Success = success;
        if(msg != null)
            Messages.Add(new(msg, type));
    }
}