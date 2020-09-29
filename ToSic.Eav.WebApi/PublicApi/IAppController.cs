using System.Net.Http;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IAppController
    {
        HttpResponseMessage Export(int appId, int zoneId, bool includeContentGroups, bool resetAppGuid);
        bool SaveData(int appId, int zoneId, bool includeContentGroups, bool resetAppGuid);
        AppExportInfoDto Statistics(int appId, int zoneId);
        ImportResultDto Import(int zoneId);
    }
}