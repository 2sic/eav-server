using System.Globalization;
using ToSic.Eav.Context;
using ToSic.Eav.WebApi.Sys.Languages;

namespace ToSic.Eav.WebApi.Sys.Admin;

/// <summary>
/// This one supplies portal-wide (or cross-portal) settings / configuration
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ZoneControllerReal(LazySvc<LanguagesBackend> languagesBackend, ISite site, LazySvc<ZoneManager> zoneManager)
    : ServiceBase("Api.ZoneRl", connect: [languagesBackend, site, zoneManager]), IZoneController
{
    public const string LogSuffix = "Zone";

    ///// <inheritdoc />
    //public IList<SiteLanguageDto> GetLanguages() => languagesBackend.Value.GetLanguages();

    /// <inheritdoc />
    public void SwitchLanguage(string cultureCode, bool enable)
    {
        var l = Log.Fn($"switch language:{cultureCode}, to:{enable}");
        // Activate or Deactivate the Culture
        zoneManager.Value.SetId(site.ZoneId).SaveLanguage(cultureCode, CultureInfo.GetCultureInfo(cultureCode).EnglishName, enable);

        //languagesBackend.Value.Toggle(cultureCode, enable, CultureInfo.GetCultureInfo(cultureCode).EnglishName);
        l.Done();
    }

    /// <inheritdoc />

}
