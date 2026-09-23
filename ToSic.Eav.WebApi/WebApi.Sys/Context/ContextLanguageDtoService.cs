using ToSic.Eav.Context.Sys;

namespace ToSic.Eav.WebApi.Sys.Context;

/// <summary>
/// Trivial helper to convert the languages into the DTO.
/// Actually only called in 2sxc (not EAV),
/// but as we'll probably move things up the stack to EAV, we'll leave this service here for now.
/// </summary>
/// <param name="appUserLanguageCheckLazy"></param>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContextLanguageDtoService(LazySvc<AppUserLanguageCheck> appUserLanguageCheckLazy)
    : ServiceBase("Bck.Admin", connect: [appUserLanguageCheckLazy])
{
    public List<ContextLanguageDto> GetLanguagesOfApp(IAppReader? appReaderOrNull)
    {
        try
        {
            var langs = appUserLanguageCheckLazy.Value.LanguagesWithPermissions(appReaderOrNull);
            var converted = langs
                .Select(lng =>
                {
                    var dto = new ContextLanguageDto
                    {
                        Code = lng.Code,
                        Culture = lng.Culture,
                        IsAllowed = lng.IsAllowed,
                        IsEnabled = lng.IsEnabled,
                        Permissions = null,
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
