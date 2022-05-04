﻿using System;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Context;
using ToSic.Eav.Logging;
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
            Lazy<ILicenseService> licenseService
            ) : base("Bck.Zones")
        {
            _appStates = appStates;
            _fingerprint = fingerprint;
            _zoneMapper = zoneMapper;
            _platform = platform;
            _site = site;
            _licenseService = licenseService;
        }
        private readonly IAppStates _appStates;
        private readonly SystemFingerprint _fingerprint;
        private readonly IZoneMapper _zoneMapper;
        private readonly IPlatformInfo _platform;
        private readonly ISite _site;
        private readonly Lazy<ILicenseService> _licenseService;

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
            // Todo: make owner contain a CSV of all owners in enabled license file(s)
            var primary = licenses.Enabled.Any() ? licenses.Enabled.First().Value : null;
            var license = new LicenseInfoDto
            {
                Count = licenses.All.Count,
                Main = "none",
                Owner = primary?.Owner
            };

            var info = new SystemInfoSetDto
            {
                License = license,
                Site = siteStats,
                System = sysInfo
            };

            return wrapLog("ok", info);
        }
    }
}
