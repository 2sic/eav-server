using System.Text.Json;
using ToSic.Eav.DataSources.Sys;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Admin;

public interface IAppController<out THttpResponse>
{
    ICollection<AppDto> List(int zoneId);

    ICollection<AppDto> InheritableApps();

    void App(int zoneId, int appId, bool fullDelete = true);

    void App(int zoneId, string name, int? inheritAppId = null);

    ICollection<SiteLanguageDto> Languages(int appId);

    AppExportInfoDto Statistics(int zoneId, int appId);

    bool FlushCache(int zoneId, int appId);

    THttpResponse Export(int zoneId, int appId, bool includeContentGroups, bool resetAppGuid, bool assetsAdam, bool assetsSite, bool assetAdamDeleted);

    bool SaveData(int zoneId, int appId, bool includeContentGroups, bool resetAppGuid, bool withPortalFiles);

    /// <summary>
    /// Get a stack of values from settings or resources
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="part">Name of the part - "settings" or "resources"</param>
    /// <param name="key">Optional key like "Settings.Images.Content.Width"</param>
    /// <param name="view">Optional guid of a view to merge with the settings</param>
    /// <returns></returns>
    List<AppStackDataRaw> GetStack(int appId, string part, string? key = null, Guid? view = null);

    /// <summary>
    /// Reset an App to the last xml state
    /// </summary>
    /// <returns></returns>
    ImportResultDto Reset(int zoneId, int appId, bool withPortalFiles);

    ImportResultDto Import(int zoneId);

    /// <summary>
    /// List all app folders in the 2sxc which:
    /// - are not installed as apps yet
    /// - have a App_Data/app.xml
    /// </summary>
    /// <param name="zoneId"></param>
    /// <returns></returns>
    IEnumerable<PendingAppDto> GetPendingApps(int zoneId);

    /// <summary>
    /// Install pending apps
    /// </summary>
    /// <param name="zoneId"></param>
    /// <param name="pendingApps"></param>
    /// <returns></returns>
    ImportResultDto InstallPendingApps(int zoneId, IEnumerable<PendingAppDto> pendingApps);

    /// <summary>
    /// Get all App Extensions and their configuration (if any)
    /// </summary>
    /// <param name="appId">App identifier</param>
    /// <returns>Object with property "extensions" containing an array of extensions</returns>
    ExtensionsResultDto Extensions(int appId);

    /// <summary>
    /// Create or replace the configuration of a specific App Extension.
    /// </summary>
    /// <param name="zoneId">Zone id (for permission/consistency)</param>
    /// <param name="appId">App identifier</param>
    /// <param name="name">Extension folder name under "/extensions"</param>
    /// <param name="configuration">JSON to write as App_Data/extension.json</param>
    /// <returns>true if saved</returns>
    bool Extensions(int zoneId, int appId, string name, JsonElement configuration);

    /// <summary>
    /// Install app extension zip
    /// </summary>
    /// <param name="zoneId">Zone id (for permission/consistency)</param>
    /// <param name="appId">App identifier</param>
    /// <param name="name">Extension folder name under "/extensions"</param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
    bool InstallExtension(int zoneId, int appId, string? name = null, bool overwrite = false);

}