using System;
using System.Collections.Generic;
using ToSic.Eav.WebApi.Dto;
#if NETFRAMEWORK
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

        /// <summary>
        /// Get a stack of values from settings or resources
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="part">Name of the part - "settings" or "resources"</param>
        /// <param name="key">Optional key like "Settings.Images.Content.Width"</param>
        /// <param name="view">Optional guid of a view to merge with the settings</param>
        /// <returns></returns>
        List<StackInfoDto> GetStack(int appId, string part, string key = null, Guid? view = null);
    }
}
