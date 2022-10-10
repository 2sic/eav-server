using System;
using System.Collections.Generic;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.Admin
{
    public interface IAppController<out THttpResponse>
    {
        List<AppDto> List(int zoneId);

        List<AppDto> InheritableApps();

        void App(int zoneId, int appId, bool fullDelete = true);

        void App(int zoneId, string name, int? inheritAppId = null);

        List<SiteLanguageDto> Languages(int appId);

        AppExportInfoDto Statistics(int zoneId, int appId);

        bool FlushCache(int zoneId, int appId);

        THttpResponse Export(int zoneId, int appId, bool includeContentGroups, bool resetAppGuid);

        bool SaveData(int zoneId, int appId, bool includeContentGroups, bool resetAppGuid);

        /// <summary>
        /// Get a stack of values from settings or resources
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="part">Name of the part - "settings" or "resources"</param>
        /// <param name="key">Optional key like "Settings.Images.Content.Width"</param>
        /// <param name="view">Optional guid of a view to merge with the settings</param>
        /// <returns></returns>
        List<StackInfoDto> GetStack(int appId, string part, string key = null, Guid? view = null);

        /// <summary>
        /// Reset an App to the last xml state
        /// </summary>
        /// <returns></returns>
        ImportResultDto Reset(int zoneId, int appId);

        ImportResultDto Import(int zoneId);

        /// <summary>
        /// List all app folders in the 2sxc which:
        /// - are not installed as apps yet
        /// - have a App_Data/app.xml
        /// </summary>
        /// <param name="zoneId"></param>
        /// <returns></returns>
        IEnumerable<PendingAppDto> GetPendingApps(int zoneId);
    }
}
