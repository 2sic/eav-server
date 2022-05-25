using System;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Context;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;
using ToSic.Eav.Security.Fingerprint;

namespace ToSic.Eav.WebApi.Zone
{
    public class ZoneBackend: HasLog<ZoneBackend>
    {
        public ZoneBackend(
            IAppStates appStates, 
            SystemFingerprint fingerprint,
            IZoneMapper zoneMapper,
            IPlatformInfo platform,
            ISite site,
            Lazy<ILicenseService> licenseService,
            LogHistory logHistory
            ) : base("Bck.Zones")
        {
            _appStates = appStates;
            _fingerprint = fingerprint;
            _zoneMapper = zoneMapper;
            _platform = platform;
            _site = site;
            _licenseService = licenseService;
            _logHistory = logHistory;
        }
        private readonly IAppStates _appStates;
        private readonly SystemFingerprint _fingerprint;
        private readonly IZoneMapper _zoneMapper;
        private readonly IPlatformInfo _platform;
        private readonly ISite _site;
        private readonly Lazy<ILicenseService> _licenseService;
        private readonly LogHistory _logHistory;

        public SystemInfoSetDto GetSystemInfo()
        {
            var wrapLog = Log.Call<SystemInfoSetDto>($"{_site.Id}");

            var zoneId = _site.ZoneId;

            var siteStats = new SiteStatsDto
            {
                SiteId = _site.Id,
                ZoneId = _site.ZoneId, 
                Apps = _appStates.Apps(zoneId).Count,
                Languages = _zoneMapper.CulturesWithState(_site).Count, 
            };

            var sysInfo = new SystemInfoDto
            {
                EavVersion = EavSystemInfo.VersionString,
                Fingerprint = _fingerprint.GetFingerprint(),
                Zones = _appStates.Zones.Count,
                Platform = _platform.Name,
                PlatformVersion = EavSystemInfo.VersionToNiceFormat(_platform.Version)
            };

            var licenses = _licenseService.Value;
            var owner = string.Join(",", licenses.Enabled
                .Select(s => s.Value.Owner)
                .Where(o => o.HasValue())
                .Distinct());
            var license = new LicenseInfoDto
            {
                Count = licenses.All.Count,
                Main = "none",
                Owner = owner
            };

            var warningsObsolete = CountInsightsMessages(Obsolete.LogObsolete.ObsoleteNameInHistory);
            var warningsOther = CountInsightsMessages(LogHistory.WarningsPrefix) - warningsObsolete;

            var warningsDto = new MessagesDto
            {
                WarningsObsolete = warningsObsolete,
                WarningsOther = warningsOther
            };

            var info = new SystemInfoSetDto
            {
                License = license,
                Site = siteStats,
                System = sysInfo,
                Messages = warningsDto
            };

            return wrapLog("ok", info);
        }

        private int CountInsightsMessages(string prefix)
        {
            var warnings = _logHistory.Logs
                .Where(l => l.Key.StartsWith(prefix))
                .Select(l => l.Value.Count)
                .Sum();
            return warnings;
        }
    }
}
