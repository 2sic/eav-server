﻿using System;
using System.IO;
using System.Linq;
#if NET451
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
#endif
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Data.Builder;
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
        public ContentImportResultDto EvaluateContent(ContentImportArgsDto args)
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
        public ContentImportResultDto ImportContent(ContentImportArgsDto args)
        {
            Log.Add("import content" + args.DebugInfo);

            var import = GetXmlImport(args);
            if (!import.ErrorLog.HasErrors)
            {
                var db = DbDataController.Instance(null, args.AppId, Log);
                import.PersistImportToRepository(db.UserName);
                SystemManager.Purge(args.AppId, Log);
            }
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
                Log.Add("import json item" + args.DebugInfo);
                var appManager = new AppManager(args.AppId, Log);
                var deserializer = new ToSic.Eav.ImportExport.Json.JsonSerializer(appManager.AppState, Log)
                {
                    // Since we're importing directly into this app, we would prefer local content-types
                    PreferLocalAppTypes = true
                };

                var entity = deserializer.Deserialize(args.GetContentString());

                entity.ResetEntityId(0);

                var checkExists = appManager.Read.Entities.Get(entity.EntityGuid);
                if (checkExists != null)
                    throw new ArgumentException("Can't import this item - an item with the same guid already exists");

                appManager.Entities.Save(entity);

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
