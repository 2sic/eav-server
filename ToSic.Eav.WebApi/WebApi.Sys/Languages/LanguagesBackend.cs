using ToSic.Eav.Context.Sys;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Languages;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class LanguagesBackend(LazySvc<AppUserLanguageCheck> appUserLanguageCheckLazy)
    : ServiceBase("Bck.Admin", connect: [appUserLanguageCheckLazy])
{
    public List<LanguageStatusRaw> GetLanguagesOfApp(IAppReader? appReaderOrNull, bool withCount = false)
    {
        try
        {
            var langs = appUserLanguageCheckLazy.Value.LanguagesWithPermissions(appReaderOrNull);
            var converted = langs
                .Select(lng =>
                {
                    var dto = new LanguageStatusRaw
                    {
                        Code = lng.Code,
                        Culture = lng.Culture,
                        IsAllowed = lng.IsAllowed,
                        IsEnabled = lng.IsEnabled,
                        Permissions = withCount ? new() { Count = lng.PermissionCount } : null,
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

}
