// 2017-10-21 temp interface to support while refactoring
// remove in 2-3 months
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Linq;
//using ToSic.Eav.Persistence.Logging;

//namespace ToSic.Eav.Apps.ImportExport.removing
//{
//    public interface IImportListTemp
//    {
//        ImportErrorLog ErrorLog { get; }
//        int Info_AmountOfEntitiesCreated { get; }
//        int Info_AmountOfEntitiesDeleted { get; }
//        int Info_AmountOfEntitiesUpdated { get; }
//        IEnumerable<string> Info_AttributeNamesInDocument { get; }
//        IEnumerable<string> Info_AttributeNamesInContentType { get; }
//        IEnumerable<string> Info_AttributeNamesNotImported { get; }
//        IEnumerable<XElement> DocumentElements { get; }
//        IEnumerable<string> Info_LanguagesInDocument { get; }

//        bool PersistImportToRepository(string userId);

//    }
//}
