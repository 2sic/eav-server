using System.Collections.Generic;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.WebApi.ImportExport
{
    public class ImportResult
    {
        public bool Succeeded;

        public List<Message> Messages;
    }

}