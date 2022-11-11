using System;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.DI;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.ImportExport.Validation;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Plumbing;
using ToSic.Eav.WebApi.Plumbing;
using ToSic.Eav.WebApi.Security;
#if NETFRAMEWORK
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
#endif

namespace ToSic.Eav.WebApi.ImportExport
{
    public class ContentExportApi<THttpResponseType> : HasLog
    {
        private AppManager _appManager;
        public ContentExportApi(
            Lazy<AppManager> appManagerLazy, 
            IAppStates appStates,
            Generator<JsonSerializer> jsonSerializer,
            ResponseMaker<THttpResponseType> responseMaker
            ) : base("Api.EaCtEx")
        {
            _appManagerLazy = appManagerLazy;
            _appStates = appStates;
            _jsonSerializer = jsonSerializer;
            _responseMaker = responseMaker;
        }
        private readonly Lazy<AppManager> _appManagerLazy;
        private readonly IAppStates _appStates;
        private readonly Generator<JsonSerializer> _jsonSerializer;
        private readonly ResponseMaker<THttpResponseType> _responseMaker;

        public ContentExportApi<THttpResponseType> Init(int appId, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            _appManager = _appManagerLazy.Value.Init(appId, Log);
            Log.A($"For app: {appId}");
            return this;
        }

        public Tuple<string, string> ExportContent(IUser user,
            string language,
            string defaultLanguage,
            string contentType,
            ExportSelection exportSelection,
            ExportResourceReferenceMode exportResourcesReferences,
            ExportLanguageResolution exportLanguageReferences,
            string selectedIds)
        {
            Log.A($"export content lang:{language}, deflang:{defaultLanguage}, ct:{contentType}, ids:{selectedIds}");
            SecurityHelpers.ThrowIfNotAdmin(user.IsSiteAdmin);

            var contextLanguages = _appStates.Languages(_appManager.ZoneId).Select(l => l.EnvironmentKey).ToArray();

            // check if we have an array of ids
            int[] ids = null;
            try
            {
                if (exportSelection == ExportSelection.Selection && !string.IsNullOrWhiteSpace(selectedIds))
                    ids = selectedIds.Split(',').Select(int.Parse).ToArray();
            }
            catch (Exception e)
            {
                throw new Exception("trouble finding selected IDs to export", e);
            }

            var tableExporter = _appManager.Entities.Exporter(contentType);
            var fileContent = exportSelection == ExportSelection.Blank
                ? tableExporter.EmptyListTemplate()
                : tableExporter.GenerateXml(language ?? "", defaultLanguage, contextLanguages, exportLanguageReferences,
                    exportResourcesReferences == ExportResourceReferenceMode.Resolve, ids);

            var contentTypeName = tableExporter.ContentType.Name;

            var fileName =
                $"2sxc {contentTypeName.Replace(" ", "-")} {language} " +
                $"{(exportSelection == ExportSelection.Blank ? "Template" : "Data")} " +
                $"{DateTime.Now:yyyyMMddHHmmss}.xml";

            return new Tuple<string, string>(fileContent, fileName);
        }

        [HttpGet]
        public THttpResponseType DownloadTypeAsJson(IUser user, string name)
        {
            Log.A($"get fields type:{name}");
            SecurityHelpers.ThrowIfNotAdmin(user.IsSiteAdmin);
            var type = _appManager.Read.ContentTypes.Get(name);
            var serializer = _jsonSerializer.New.Init(_appManager.AppState, Log);
            var fileName = (type.Scope + "." + type.NameId + ImpExpConstants.Extension(ImpExpConstants.Files.json))
                .RemoveNonFilenameCharacters();
 
            return _responseMaker.File(serializer.Serialize(type), fileName, MimeHelper.Json);
        }

        [HttpGet]
        public THttpResponseType DownloadEntityAsJson(IUser user, int id, string prefix, bool withMetadata)
        {
            Log.A($"get fields id:{id}");
            SecurityHelpers.ThrowIfNotAdmin(user.IsSiteAdmin);
            var entity = _appManager.Read.Entities.Get(id);
            var serializer = _jsonSerializer.New.Init(_appManager.AppState, Log);

            return _responseMaker.File(
                serializer.Serialize(entity, withMetadata ? FileSystemLoader.QueryMetadataDepth : 0),
                (prefix + (string.IsNullOrWhiteSpace(prefix) ? "" : ".")
                 + entity.GetBestTitle() + ImpExpConstants.Extension(ImpExpConstants.Files.json))
                .RemoveNonFilenameCharacters());
        }

    }
}
