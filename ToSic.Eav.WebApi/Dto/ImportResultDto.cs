using System.Collections.Generic;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.WebApi.Dto
{
    public class ImportResultDto
    {
        public bool Succeeded;

        public List<Message> Messages;
    }

}