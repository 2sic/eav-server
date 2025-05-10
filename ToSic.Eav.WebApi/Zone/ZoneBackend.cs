using ToSic.Eav.Context;
using ToSic.Eav.Integration;
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Lib.Code.InfoSystem;

namespace ToSic.Eav.WebApi.Zone;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ZoneBackend(
    IAppsCatalog appsCatalog,
    SystemFingerprint fingerprint,
    IZoneMapper zoneMapper,
    IPlatformInfo platform,
    ISite site,
    LazySvc<ILicenseService> licenseService,
    ILogStoreLive logStore)
    : ServiceBase("Bck.Zones", connect: [appsCatalog, fingerprint, zoneMapper, platform, site, licenseService, logStore])
{
    public SystemInfoSetDto GetSystemInfo()
    {
        var l = Log.Fn<SystemInfoSetDto>($"{site.Id}");

        var zoneId = site.ZoneId;

        var siteStats = new SiteStatsDto
        {
            SiteId = site.Id,
            ZoneId = site.ZoneId,
            Apps = appsCatalog.Apps(zoneId).Count,
            Languages = zoneMapper.CulturesWithState(site).Count,
        };

        var sysInfo = new SystemInfoDto
        {
            EavVersion = EavSystemInfo.VersionString,
            Fingerprint = fingerprint.GetFingerprint(),
            Zones = appsCatalog.Zones.Count,
            Platform = platform.Name,
            PlatformVersion = EavSystemInfo.VersionToNiceFormat(platform.Version)
        };

        var licenses = licenseService.Value;

        // owner is coma separated list of all owners from enabled licenses 
        var owner = string.Join(", ", licenses.All
            .Where(ls => ls.IsEnabled)
            .Select(ls => ls.Owner)
            .Where(o => o.HasValue())
            .Distinct());
        var license = new LicenseInfoDto
        {
            Count = licenses.All.Count,
            Main = "none",
            Owner = owner
        };

        var warningsObsolete = CountInsightsMessages(CodeInfoConstants.ObsoleteNameInHistory);
        var warningsOther = CountInsightsMessages(LogConstants.StoreWarningsPrefix) - warningsObsolete;

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

        return l.ReturnAsOk(info);
    }

    private int CountInsightsMessages(string prefix)
    {
        var warnings = logStore.Segments
            .Where(l => l.Key.StartsWith(prefix))
            .Select(l => l.Value.Count)
            .Sum();
        return warnings;
    }
}