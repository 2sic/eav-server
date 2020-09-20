using System.Net.Http;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IAppController
    {
        HttpResponseMessage ExportApp(int appId, int zoneId, bool includeContentGroups, bool resetAppGuid);
        //HttpResponseMessage ExportParts(int appId, int zoneId, string contentTypeIdsString, string entityIdsString, string templateIdsString);
        bool SaveToDotData(int appId, int zoneId, bool includeContentGroups, bool resetAppGuid);
        AppExportInfoDto GetAppInfo(int appId, int zoneId);
        //ExportPartsOverviewDto GetParts(int appId, int zoneId, string scope);
        ImportResultDto ImportApp();
        //ImportResultDto ImportParts();
    }
}