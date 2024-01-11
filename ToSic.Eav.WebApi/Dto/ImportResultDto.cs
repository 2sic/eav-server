using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.WebApi.Dto;

public class ImportResultDto
{
    public bool Success;

    public List<Message> Messages = new();

    public ImportResultDto(bool success = false, string msg = null, Message.MessageTypes type = Message.MessageTypes.Information)
    {
        Success = success;
        if(msg != null) Messages.Add(new Message(msg, type));
    }
}