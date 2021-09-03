using System;
using System.Linq;
using System.Net.Http;
#if NET451
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
#endif
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.ImportExport.Validation;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;
using ToSic.Eav.WebApi.Helpers;
using ToSic.Eav.WebApi.Security;

namespace ToSic.Eav.WebApi
{
    public class ContentExportApi : HasLog
    {
        private AppManager _appManager;
        public ContentExportApi(Lazy<AppManager> appManagerLazy, IAppStates appStates) : base("Api.EaCtEx")
        {
            _appManagerLazy = appManagerLazy;
            _appStates = appStates;
        }
        private readonly Lazy<AppManager> _appManagerLazy;
        private readonly IAppStates _appStates;

        public ContentExportApi Init(int appId, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            _appManager = _appManagerLazy.Value.Init(appId, Log);
            Log.Add($"For app: {appId}");
            return this;
        }

        public HttpResponseMessage ExportContent(IUser user, string language, 
            string defaultLanguage, 
            string contentType,
            ExportSelection exportSelection, 
            ExportResourceReferenceMode exportResourcesReferences,
            ExportLanguageResolution exportLanguageReferences, 
            string selectedIds)
        {
            Log.Add($"export content lang:{language}, deflang:{defaultLanguage}, ct:{contentType}, ids:{selectedIds}");
            SecurityHelpers.ThrowIfNotAdmin(user);

            //var appManager = new AppManager(appId, Log);
            var contextLanguages = _appStates.Languages(_appManager.ZoneId) /*_appManager.Read.Zone.Languages()*/.Select(l => l.EnvironmentKey).ToArray();

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

            return Download.BuildDownload(fileContent, fileName);

        }

        [HttpGet]
        public HttpResponseMessage DownloadTypeAsJson(IUser user, string name)
        {
            Log.Add($"get fields type:{name}");
            SecurityHelpers.ThrowIfNotAdmin(user);
            var type = _appManager.Read.ContentTypes.Get(name);
            var serializer = _appManager.ServiceProvider.Build<JsonSerializer>().Init(_appManager.AppState, Log);

            return Download.BuildDownload(serializer.Serialize(type),
                (type.Scope + "." + type.StaticName + ImpExpConstants.Extension(ImpExpConstants.Files.json))
                     .RemoveNonFilenameCharacters());
        }

        [HttpGet]
        public HttpResponseMessage DownloadEntityAsJson(IUser user, int id, string prefix, bool withMetadata)
        {
            Log.Add($"get fields id:{id}");
            SecurityHelpers.ThrowIfNotAdmin(user);
            var entity = _appManager.Read.Entities.Get(id);
            var serializer = _appManager.ServiceProvider.Build<JsonSerializer>().Init(_appManager.AppState, Log);

            return Download.BuildDownload(
                serializer.Serialize(entity, withMetadata ? FileSystemLoader.QueryMetadataDepth : 0),
                (prefix + (string.IsNullOrWhiteSpace(prefix) ? "" : ".")
                 + entity.GetBestTitle() + ImpExpConstants.Extension(ImpExpConstants.Files.json))
                .RemoveNonFilenameCharacters());
        }

    }
}
