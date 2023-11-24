using System.Collections.Generic;
using System.Globalization;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.WebApi.Languages;
using ToSic.Eav.WebApi.Zone;
using ServiceBase = ToSic.Lib.Services.ServiceBase;

namespace ToSic.Eav.WebApi.Admin;

/// <summary>
/// This one supplies portal-wide (or cross-portal) settings / configuration
/// </summary>
public class ZoneControllerReal : ServiceBase, IZoneController
{
    public const string LogSuffix = "Zone";
    public ZoneControllerReal(LazySvc<LanguagesBackend> languagesBackend, LazySvc<ZoneBackend> zoneBackend): base("Api.ZoneRl") =>
        ConnectServices(
            _languagesBackend = languagesBackend,
            _zoneBackend = zoneBackend
        );
    private readonly LazySvc<LanguagesBackend> _languagesBackend;
    private readonly LazySvc<ZoneBackend> _zoneBackend;

    /// <inheritdoc />
    public IList<SiteLanguageDto> GetLanguages() => _languagesBackend.Value.GetLanguages();

    /// <inheritdoc />
    public void SwitchLanguage(string cultureCode, bool enable) 
        => _languagesBackend.Value.Toggle(cultureCode, enable, CultureInfo.GetCultureInfo(cultureCode).EnglishName);

    /// <inheritdoc />
    public SystemInfoSetDto GetSystemInfo() => _zoneBackend.Value.GetSystemInfo();

}