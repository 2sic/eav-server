using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Logging;
using ToSic.Eav.WebApi.Dto;
#if NETFRAMEWORK
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
#endif

namespace ToSic.Eav.WebApi.ImportExport
{
    /// <inheritdoc />
    public class ContentImportApi : HasLog
    {
        public ContentImportApi(Lazy<AppManager> appManagerLazy, Lazy<JsonSerializer> jsonSerializerLazy, SystemManager systemManager, IAppStates appStates) : base("Api.EaCtIm")
        {
            _appManagerLazy = appManagerLazy;
            _jsonSerializerLazy = jsonSerializerLazy;
            _systemManager = systemManager;
            _appStates = appStates;
        }
        private readonly Lazy<AppManager> _appManagerLazy;
        private readonly Lazy<JsonSerializer> _jsonSerializerLazy;
        private readonly SystemManager _systemManager;
        private readonly IAppStates _appStates;
        private AppManager _appManager;

        public ContentImportApi Init(int appId, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            _appManager = _appManagerLazy.Value.Init(appId, Log);
            Log.A($"For app: {appId}");
            return this;
        }


        [HttpPost]
        public ContentImportResultDto XmlPreview(ContentImportArgsDto args)
        {
            Log.A("eval content - start" + args.DebugInfo);

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
            var wrapLog = Log.Fn<ContentImportResultDto>(args.DebugInfo);

            var import = GetXmlImport(args);
            if (!import.ErrorLog.HasErrors)
            {
                import.PersistImportToRepository();
                _systemManager.Init(Log).PurgeApp(args.AppId);
            }

            return wrapLog.Return(new ContentImportResultDto(!import.ErrorLog.HasErrors, null), "done, errors: " + import.ErrorLog.HasErrors);
        }

        private ImportListXml GetXmlImport(ContentImportArgsDto args)
        {
            Log.A("get xml import " + args.DebugInfo);
            var contextLanguages = _appStates.Languages(_appManager.ZoneId) /*_appManager.Read.Zone.Languages()*/.Select(l => l.EnvironmentKey).ToArray();

            using (var contentSteam = new MemoryStream(global::System.Convert.FromBase64String(args.ContentBase64)))
            {
                return _appManager.Entities.Importer(args.ContentType, contentSteam,
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
                var deserializer = _jsonSerializerLazy.Value.Init(_appManager.AppState, Log);
                // Since we're importing directly into this app, we prefer local content-types
                deserializer.PreferLocalAppTypes = true;

                _appManager.Entities.Import(new List<IEntity> {deserializer.Deserialize(args.GetContentString()) });
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
