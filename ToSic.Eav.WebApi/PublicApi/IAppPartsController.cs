using System.Net.Http;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IAppPartsController
    {
        HttpResponseMessage Export(int zoneId, int appId, string contentTypeIdsString, string entityIdsString, string templateIdsString);
        ExportPartsOverviewDto Get(int zoneId, int appId, string scope);
        ImportResultDto Import(int zoneId, int appId);
    }
}