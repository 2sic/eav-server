using System.Net.Http;
using ToSic.Eav.ImportExport.Options;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IContentExportController
    {
        //HttpResponseMessage DownloadEntityAsJson(int appId, int id, string prefix, bool withMetadata);
        //HttpResponseMessage DownloadTypeAsJson(int appId, string name);
        HttpResponseMessage ExportContent(int appId, string language, string defaultLanguage, string contentType, ExportSelection recordExport, ExportResourceReferenceMode resourcesReferences, ExportLanguageResolution languageReferences, string selectedIds = null);
    }
}