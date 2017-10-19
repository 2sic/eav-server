using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.Apps.ImportExport.removing
{
    public interface IImportListTemp
    {
        ImportErrorLog ErrorLog { get; }
        int AmountOfEntitiesCreated { get; }
        int AmountOfEntitiesDeleted { get; }
        int AmountOfEntitiesUpdated { get; }
        IEnumerable<string> AttributeNamesInDocument { get; }
        IEnumerable<string> AttributeNamesInContentType { get; }
        IEnumerable<string> AttributeNamesNotImported { get; }
        IEnumerable<XElement> DocumentElements { get; }
        IEnumerable<string> LanguagesInDocument { get; }

        bool PersistImportToRepository(string userId);

    }
}
