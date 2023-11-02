using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Serialization;
using ToSic.Lib.Logging;
using ToSic.Eav.WebApi.Dto;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using ToSic.Eav.Apps.Work;

#if NETFRAMEWORK
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
#endif

namespace ToSic.Eav.WebApi.ImportExport
{
    /// <inheritdoc />
    public class ContentImportApi : ServiceBase
    {
        private readonly LazySvc<ImportListXml> _importListXml;

        public ContentImportApi(LazySvc<ImportListXml> importListXml, AppWork appWork, LazySvc<JsonSerializer> jsonSerializerLazy, SystemManager systemManager, IAppStates appStates) : base("Api.EaCtIm")
        {
            ConnectServices(
                _appWork = appWork,
                _importListXml = importListXml,
                _jsonSerializerLazy = jsonSerializerLazy,
                _systemManager = systemManager,
                _appStates = appStates
            );
        }
        private readonly AppWork _appWork;
        private readonly LazySvc<JsonSerializer> _jsonSerializerLazy;
        private readonly SystemManager _systemManager;
        private readonly IAppStates _appStates;
        private IAppWorkCtx AppWorkCtx { get; set; }

        public ContentImportApi Init(int appId)
        {
            var l = Log.Fn<ContentImportApi>($"app: {appId}");
            AppWorkCtx = _appWork.Context(appId);
            return l.Return(this);
        }


        [HttpPost]
        public ContentImportResultDto XmlPreview(ContentImportArgsDto args)
        {
            var l = Log.Fn<ContentImportResultDto>("eval content - start" + args.DebugInfo);

            var import = GetXmlImport(args);
            var result = import.ErrorLog.HasErrors
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
            return l.Return(result);
        }

        [HttpPost]
        public ContentImportResultDto XmlImport(ContentImportArgsDto args)
        {
            var l = Log.Fn<ContentImportResultDto>(args.DebugInfo);
            
            var import = GetXmlImport(args);
            if (!import.ErrorLog.HasErrors)
            {
                import.PersistImportToRepository();
                _systemManager.PurgeApp(args.AppId);
            }

            return l.Return(new ContentImportResultDto(!import.ErrorLog.HasErrors, null), "done, errors: " + import.ErrorLog.HasErrors);
        }

        private ImportListXml GetXmlImport(ContentImportArgsDto args)
        {
            var l = Log.Fn<ImportListXml>("get xml import " + args.DebugInfo);
            var contextLanguages = _appStates.Languages(AppWorkCtx.ZoneId).Select(lng => lng.EnvironmentKey).ToArray();

            using (var contentSteam = new MemoryStream(Convert.FromBase64String(args.ContentBase64)))
            {
                var importer = _importListXml.Value.Init(AppWorkCtx.AppState, args.ContentType, contentSteam,
                    contextLanguages, args.DefaultLanguage,
                    args.ClearEntities, args.ImportResourcesReferences);
                return l.Return(importer);
            }
        }

        [HttpPost]
        public bool Import(EntityImportDto args)
        {
            var l = Log.Fn<bool>(message: "import json item" + args.DebugInfo);
            try
            {
                var deserializer = _jsonSerializerLazy.Value.SetApp(AppWorkCtx.AppState);
                // Since we're importing directly into this app, we prefer local content-types
                deserializer.PreferLocalAppTypes = true;

                var listToImport = new List<IEntity> { deserializer.Deserialize(args.GetContentString()) };

                _appWork.EntitySave(AppWorkCtx.AppState).Import(listToImport);

                return l.ReturnTrue();
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                l.Ex(ex);
                throw new Exception("Couldn't import - probably bad file format", ex);
            }
        }
    }








}
