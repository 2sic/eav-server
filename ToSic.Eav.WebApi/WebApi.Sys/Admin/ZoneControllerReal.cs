using System.Globalization;
using ToSic.Eav.WebApi.Languages;
using ToSic.Eav.WebApi.Zone;
using ServiceBase = ToSic.Lib.Services.ServiceBase;

namespace ToSic.Eav.WebApi.Admin;

/// <summary>
/// This one supplies portal-wide (or cross-portal) settings / configuration
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ZoneControllerReal(LazySvc<LanguagesBackend> languagesBackend, LazySvc<ZoneBackend> zoneBackend)
    : ServiceBase("Api.ZoneRl", connect: [languagesBackend, zoneBackend]), IZoneController
{
    public const string LogSuffix = "Zone";

    /// <inheritdoc />
    public IList<SiteLanguageDto> GetLanguages() => languagesBackend.Value.GetLanguages();

    /// <inheritdoc />
    public void SwitchLanguage(string cultureCode, bool enable) 
        => languagesBackend.Value.Toggle(cultureCode, enable, CultureInfo.GetCultureInfo(cultureCode).EnglishName);

    /// <inheritdoc />
    public SystemInfoSetDto GetSystemInfo() => zoneBackend.Value.GetSystemInfo();

}