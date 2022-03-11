using System.Collections.Generic;
using System.Globalization;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.WebApi.Languages;
using ToSic.Eav.WebApi.Zone;

namespace ToSic.Eav.WebApi.Admin
{
    /// <summary>
    /// This one supplies portal-wide (or cross-portal) settings / configuration
    /// </summary>
    public class ZoneControllerReal : HasLog<ZoneControllerReal>, IZoneController
    {
        public const string LogSuffix = "Zone";
        public ZoneControllerReal(LazyInitLog<LanguagesBackend> languagesBackend, LazyInitLog<ZoneBackend> zoneBackend): base("Api.ZoneRl")
        {
            _languagesBackend = languagesBackend.SetLog(Log);
            _zoneBackend = zoneBackend.SetLog(Log);
        }
        private readonly LazyInitLog<LanguagesBackend> _languagesBackend;
        private readonly LazyInitLog<ZoneBackend> _zoneBackend;

        /// <inheritdoc />
        public IList<SiteLanguageDto> GetLanguages() => _languagesBackend.Ready.GetLanguages();

        /// <inheritdoc />
        public void SwitchLanguage(string cultureCode, bool enable) 
            => _languagesBackend.Ready.Toggle(cultureCode, enable, CultureInfo.GetCultureInfo(cultureCode).EnglishName);

        /// <inheritdoc />
        public SystemInfoSetDto GetSystemInfo() => _zoneBackend.Ready.GetSystemInfo();

    }
}