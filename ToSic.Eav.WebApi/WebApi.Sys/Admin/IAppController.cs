using ToSic.Eav.DataSources.Sys.Internal;

namespace ToSic.Eav.WebApi.Admin;

public interface IAppController<out THttpResponse>
{
    List<AppDto> List(int zoneId);

    List<AppDto> InheritableApps();

    void App(int zoneId, int appId, bool fullDelete = true);

    void App(int zoneId, string name, int? inheritAppId = null, int templateId = 0);

    List<SiteLanguageDto> Languages(int appId);

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
    List<AppStackDataRaw> GetStack(int appId, string part, string key = null, Guid? view = null);

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
}