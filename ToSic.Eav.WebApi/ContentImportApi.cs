using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if NET451
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
#endif
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi
{
    /// <inheritdoc />
    public class ContentImportApi : HasLog
    {
        public ContentImportApi(ILog parentLog = null) : base("Api.EaCtIm", parentLog)
        {
        }


        [HttpPost]
        public ContentImportResultDto XmlPreview(ContentImportArgsDto args)
        {
            Log.Add("eval content - start" + args.DebugInfo);

            var import = GetXmlImport(args);
            return import.ErrorLog.HasErrors 
                ? new ContentImportResultDto(!import.ErrorLog.HasErrors, import.ErrorLog.Errors) 
                : new ContentImportResultDto(!import.ErrorLog.HasErrors, new ImportStatisticsDto
                {
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
        public ContentImportResultDto XmlImport(ContentImportArgsDto args)
        {
            var wrapLog = Log.Call(args.DebugInfo);

            var import = GetXmlImport(args);
            if (!import.ErrorLog.HasErrors)
            {
                import.PersistImportToRepository();
                SystemManager.Purge(args.AppId, Log);
            }

            wrapLog("done, errors: " + import.ErrorLog.HasErrors);
            return new ContentImportResultDto(!import.ErrorLog.HasErrors, null);
        }

        private ImportListXml GetXmlImport(ContentImportArgsDto args)
        {
            Log.Add("get xml import " + args.DebugInfo);
            var appManager = new AppManager(args.AppId, Log);
            var contextLanguages = appManager.Read.Zone.Languages().Select(l => l.EnvironmentKey).ToArray();

            using (var contentSteam = new MemoryStream(Convert.FromBase64String(args.ContentBase64)))
            {
                return appManager.Entities.Importer(args.ContentType, contentSteam,
                    contextLanguages, args.DefaultLanguage,
                    args.ClearEntities, args.ImportResourcesReferences);
            }
        }

        [HttpPost]
        public bool Import(EntityImportDto args)
        {
            try
            {
                var callLog = Log.Call<bool>(null, "import json item" + args.DebugInfo);
                var appManager = new AppManager(args.AppId, Log);
                var deserializer = new ImportExport.Json.JsonSerializer(appManager.AppState, Log)
                {
                    PreferLocalAppTypes = true, // Since we're importing directly into this app, we prefer local content-types
                };

                appManager.Entities.Import(new List<IEntity> {deserializer.Deserialize(args.GetContentString()) });
                return callLog("ok", true);
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
