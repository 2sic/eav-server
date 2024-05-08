using ToSic.Eav.Apps.State;
using ToSic.Eav.Cms.Internal.Languages;
using ToSic.Eav.Context;
using ToSic.Eav.Integration;
using ServiceBase = ToSic.Lib.Services.ServiceBase;

namespace ToSic.Eav.WebApi.Languages;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LanguagesBackend(
    LazySvc<IZoneMapper> zoneMapper,
    LazySvc<ZoneManager> zoneManager,
    ISite site,
    LazySvc<AppUserLanguageCheck> appUserLanguageCheckLazy)
    : ServiceBase("Bck.Admin", connect: [zoneManager, site, appUserLanguageCheckLazy, zoneMapper])
{
    public IList<SiteLanguageDto> GetLanguages()
    {
        var l = Log.Fn<IList<SiteLanguageDto>>($"{site.Id}");
        // ReSharper disable once PossibleInvalidOperationException
        var cultures = zoneMapper.Value.CulturesWithState(site)
            .Select(c => new SiteLanguageDto { Code = c.Code, Culture = c.Culture, IsEnabled = c.IsEnabled })
            .ToList();

        return l.Return(cultures, "found:" + cultures.Count);
    }

    public List<SiteLanguageDto> GetLanguagesOfApp(IAppStateInternal appStateOrNull, bool withCount = false)
    {
        try
        {
            var langs = appUserLanguageCheckLazy.Value.LanguagesWithPermissions(appStateOrNull);
            var converted = langs.Select(l =>
                {
                    var dto = new SiteLanguageDto { Code = l.Code, Culture = l.Culture, IsAllowed = l.IsAllowed, IsEnabled = l.IsEnabled };
                    if (withCount) dto.Permissions = new() { Count = l.PermissionCount };
                    return dto;
                })
                .ToList();
            return converted;
        }
        catch (Exception ex)
        {
            Log.Ex(ex);
            return new();
        }

    }

    public void Toggle(string cultureCode, bool enable, string niceName)
    {
        Log.A($"switch language:{cultureCode}, to:{enable}");
        // Activate or Deactivate the Culture
        zoneManager.Value.SetId(site.ZoneId).SaveLanguage(cultureCode, niceName, enable);
    }
}