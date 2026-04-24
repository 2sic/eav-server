using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys;
using ToSic.Eav.WebApi.Sys.Dto;
using Services_ServiceBase = ToSic.Sys.Services.ServiceBase;

namespace ToSic.Eav.WebApi.Sys.Languages;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class LanguagesBackend(
    LazySvc<ZoneManager> zoneManager,
    ISite site,
    LazySvc<AppUserLanguageCheck> appUserLanguageCheckLazy)
    : Services_ServiceBase("Bck.Admin", connect: [zoneManager, site, appUserLanguageCheckLazy])
{
    public List<SiteLanguageDto> GetLanguagesOfApp(IAppReader? appReaderOrNull, bool withCount = false)
    {
        try
        {
            var langs = appUserLanguageCheckLazy.Value.LanguagesWithPermissions(appReaderOrNull);
            var converted = langs
                .Select(l =>
                {
                    var dto = new SiteLanguageDto
                    {
                        Code = l.Code,
                        Culture = l.Culture,
                        IsAllowed = l.IsAllowed,
                        IsEnabled = l.IsEnabled,
                        Permissions = (withCount) ? new() { Count = l.PermissionCount } : null,
                    };
                    return dto;
                })
                .ToList();
            return converted;
        }
        catch (Exception ex)
        {
            Log.Ex(ex);
            return [];
        }

    }

    public void Toggle(string cultureCode, bool enable, string niceName)
    {
        Log.A($"switch language:{cultureCode}, to:{enable}");
        // Activate or Deactivate the Culture
        zoneManager.Value.SetId(site.ZoneId).SaveLanguage(cultureCode, niceName, enable);
    }
}