using System.Collections.Generic;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.WebApi.Dto
{
    public class ImportResultDto
    {
        public bool Succeeded;

        public List<Message> Messages = new List<Message>();

        public ImportResultDto(bool success = false, string msg = null, Message.MessageTypes type = Message.MessageTypes.Information)
        {
            Succeeded = success;
            if(msg != null) Messages.Add(new Message(msg, type));
        }
    }

}