using System.Net.Http;
using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.WebApi.ImportExport;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IImportExportController
    {
        HttpResponseMessage ExportApp(int appId, int zoneId, bool includeContentGroups, bool resetAppGuid);
        HttpResponseMessage ExportContent(int appId, int zoneId, string contentTypeIdsString, string entityIdsString, string templateIdsString);
        bool ExportForVersionControl(int appId, int zoneId, bool includeContentGroups, bool resetAppGuid);
        AppExportInfoDto GetAppInfo(int appId, int zoneId);
        ExportPartsOverviewDto GetContentInfo(int appId, int zoneId, string scope);
        ImportResult ImportApp();
        ImportResult ImportContent();
    }
}