﻿using System;
using System.Collections.Generic;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IAppController<out THttpResponse>
    {
        List<AppDto> List(int zoneId);

        List<AppDto> InheritableApps();

        THttpResponse Export(int zoneId, int appId, bool includeContentGroups, bool resetAppGuid);
        bool SaveData(int zoneId, int appId, bool includeContentGroups, bool resetAppGuid);
        AppExportInfoDto Statistics(int zoneId, int appId);
        ImportResultDto Import(int zoneId);

        List<SiteLanguageDto> Languages(int appId);

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
