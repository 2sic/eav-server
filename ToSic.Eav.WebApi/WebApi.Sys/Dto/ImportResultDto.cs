using ToSic.Eav.Persistence.Sys.Logging;

namespace ToSic.Eav.WebApi.Sys.Dto;

public class ImportResultDto
{
    public bool Success;

    public List<Message> Messages = [];

    public ImportResultDto(bool success = false, string msg = null, Message.MessageTypes type = Message.MessageTypes.Information)
    {
        Success = success;
        if(msg != null) Messages.Add(new(msg, type));
    }
}