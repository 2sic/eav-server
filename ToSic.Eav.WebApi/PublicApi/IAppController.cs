using System;
using System.Net.Http;
#if NETSTANDARD
using Microsoft.AspNetCore.Mvc;
#endif
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IAppController
    {
#if NETSTANDARD
        IActionResult Export(int appId, int zoneId, bool includeContentGroups, bool resetAppGuid);
#else
        HttpResponseMessage Export(int appId, int zoneId, bool includeContentGroups, bool resetAppGuid);
#endif
        bool SaveData(int appId, int zoneId, bool includeContentGroups, bool resetAppGuid);
        AppExportInfoDto Statistics(int appId, int zoneId);
        ImportResultDto Import(int zoneId);
    }
}
