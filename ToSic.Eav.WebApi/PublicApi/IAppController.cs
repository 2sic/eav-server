using System.Collections.Generic;
using ToSic.Eav.WebApi.Dto;
#if NET451
using ExportResponse = System.Net.Http.HttpResponseMessage;
#else
using ExportResponse = Microsoft.AspNetCore.Mvc.IActionResult;
#endif

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IAppController
    {
        List<AppDto> List(int zoneId);

        ExportResponse Export(int appId, int zoneId, bool includeContentGroups, bool resetAppGuid);
        bool SaveData(int appId, int zoneId, bool includeContentGroups, bool resetAppGuid);
        AppExportInfoDto Statistics(int appId, int zoneId);
        ImportResultDto Import(int zoneId);

        /// <summary>
        /// Reset an App to the last xml state
        /// </summary>
        /// <returns></returns>
        ImportResultDto Reset(int zoneId, int appId);
    }
}
