using ToSic.Eav.Persistence.Sys.Logging;

namespace ToSic.Eav.ImportExport.Sys;

public class ImportResultDto
{
    public bool Success;

    public List<Message> Messages = [];

    public ImportResultDto() {}


    public ImportResultDto(bool success = false, string? msg = null, Message.MessageTypes type = Message.MessageTypes.Information)
    {
        Success = success;
        if(msg != null)
            Messages.Add(new(msg, type));
    }
}