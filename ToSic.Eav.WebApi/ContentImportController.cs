using System;
using System.IO;
using System.Linq;
using System.Web.Http;
using ToSic.Eav.Apps;
using ToSic.Eav.BLL.Parts;
using ToSic.Eav.ImportExport.Options;



namespace ToSic.Eav.WebApi
{
    public class ContentImportController : Eav3WebApiBase
    {
        public class ContentImportArgs
        {
            public int AppId;

            public string DefaultLanguage;

            public ImportResourceReferenceMode ImportResourcesReferences;

            public ImportDeleteUnmentionedItems ClearEntities;

            public string ContentType;

            public string ContentBase64;
        }

        public class ContentImportResult
        {
            public bool Succeeded;

            public dynamic Detail;

            public ContentImportResult(bool succeeded, dynamic detail)
            {
                Succeeded = succeeded;
                Detail = detail;
            }
        }


        [HttpPost]
        public ContentImportResult EvaluateContent(ContentImportArgs args)
        {
            AppId = args.AppId;

            var import = GetXmlImport(args);
            return import.ErrorLog.HasErrors 
                ? new ContentImportResult(!import.ErrorLog.HasErrors, import.ErrorLog.Errors) 
                : new ContentImportResult(!import.ErrorLog.HasErrors, new {
                    import.AmountOfEntitiesCreated,
                    import.AmountOfEntitiesDeleted,
                    import.AmountOfEntitiesUpdated,
                    import.AttributeNamesInDocument,
                    import.AttributeNamesInContentType,
                    import.AttributeNamesNotImported,
                    DocumentElementsCount = import.DocumentElements.Count(),
                    LanguagesInDocumentCount = import.LanguagesInDocument.Count()
                });
        }

        [HttpPost]
        public ContentImportResult ImportContent(ContentImportArgs args)
        {
            AppId = args.AppId;

            var import = GetXmlImport(args);
            if (!import.ErrorLog.HasErrors)
            {
                import.PersistImportToRepository(CurrentContext.UserName);
                SystemManager.Purge(AppId);
            }
            return new ContentImportResult(!import.ErrorLog.HasErrors, null);
        }


        private DbXmlImportVTable GetXmlImport(ContentImportArgs args)
        {
            var contentTypeId = CurrentContext.AttribSet.GetAttributeSetWithEitherName(args.ContentType).AttributeSetID;// GetContentTypeId(args.ContentType);
            var contextLanguages = GetContextLanguages();

            using (var contentSteam = new MemoryStream(Convert.FromBase64String(args.ContentBase64)))
            {
                return new DbXmlImportVTable(CurrentContext.ZoneId, args.AppId, contentTypeId, contentSteam, contextLanguages, args.DefaultLanguage, args.ClearEntities, args.ImportResourcesReferences);
            }
        }

        private string[] GetContextLanguages()
        {
            return CurrentContext.Dimensions.GetLanguages().Select(language => language.ExternalKey).ToArray();
        }

        //private int GetContentTypeId(string name)
        //{
        //    return CurrentContext.AttribSet.GetAttributeSetId(name, null);
        //}
    }
}
