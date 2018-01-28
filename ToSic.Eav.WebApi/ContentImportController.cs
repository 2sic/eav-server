using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Http;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.WebApi.Formats;

namespace ToSic.Eav.WebApi
{
    /// <inheritdoc />
    public class ContentImportController : Eav3WebApiBase
    {
        public ContentImportController(Log parentLog = null) : base(parentLog)
        {
            Log.Rename("EaCtIm");
        }


        [HttpPost]
        public ContentImportResult EvaluateContent(ContentImportArgs args)
        {
            Log.Add("eval content - start" + args.DebugInfo);
            AppId = args.AppId;

            var import = GetXmlImport(args);
            return import.ErrorLog.HasErrors 
                ? new ContentImportResult(!import.ErrorLog.HasErrors, import.ErrorLog.Errors) 
                : new ContentImportResult(!import.ErrorLog.HasErrors, new {
                    AmountOfEntitiesCreated = import.Info_AmountOfEntitiesCreated,
                    AmountOfEntitiesDeleted = import.Info_AmountOfEntitiesDeleted,
                    AmountOfEntitiesUpdated = import.Info_AmountOfEntitiesUpdated,
                    AttributeNamesInDocument = import.Info_AttributeNamesInDocument,
                    AttributeNamesInContentType = import.Info_AttributeNamesInContentType,
                    AttributeNamesNotImported = import.Info_AttributeNamesNotImported,
                    DocumentElementsCount = import.DocumentElements.Count(),
                    LanguagesInDocumentCount = import.Info_LanguagesInDocument.Count()
                });
        }

        [HttpPost]
        public ContentImportResult ImportContent(ContentImportArgs args)
        {
            Log.Add("import content" + args.DebugInfo);
            AppId = args.AppId;

            var import = GetXmlImport(args);
            if (!import.ErrorLog.HasErrors)
            {
                import.PersistImportToRepository(CurrentContext.UserName);
                SystemManager.Purge(AppId);
            }
            return new ContentImportResult(!import.ErrorLog.HasErrors, null);
        }

        private ImportListXml GetXmlImport(ContentImportArgs args)
        {
            Log.Add("get xml import " + args.DebugInfo);
            var contextLanguages = AppManager.Read.Zone.Languages().Select(l => l.EnvironmentKey).ToArray();

            using (var contentSteam = new MemoryStream(Convert.FromBase64String(args.ContentBase64)))
            {
                return AppManager.Entities.Importer(args.ContentType, contentSteam,
                    contextLanguages, args.DefaultLanguage,
                    args.ClearEntities, args.ImportResourcesReferences);
            }
        }

        [HttpPost]
        public bool Import(EntityImport args)
        {
            try
            {
                Log.Add("import json item" + args.DebugInfo);
                AppId = args.AppId;

                var deser = new ImportExport.Json.JsonSerializer(AppManager.Package, Log);
                var entity = deser.Deserialize(args.GetContentString());

                entity.ResetEntityId(0);

                var checkExists = AppManager.Read.Entities.Get(entity.EntityGuid);
                if (checkExists != null)
                    throw new ArgumentException("Can't import this item - an item with the same guid already exists");

                AppManager.Entities.Save(entity);

                return true;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't import - probably bad file format", ex);
            }
        }
    }








}
