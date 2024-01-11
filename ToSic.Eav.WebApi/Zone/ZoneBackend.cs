using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Code.InfoSystem;
using ToSic.Eav.Context;
using ToSic.Eav.Integration;
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.WebApi.Zone;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ZoneBackend: ServiceBase
{
    public ZoneBackend(
        IAppStates appStates, 
        SystemFingerprint fingerprint,
        IZoneMapper zoneMapper,
        IPlatformInfo platform,
        ISite site,
        LazySvc<ILicenseService> licenseService,
        ILogStoreLive logStore
    ) : base("Bck.Zones") =>
        ConnectServices(
            _appStates = appStates,
            _fingerprint = fingerprint,
            _zoneMapper = zoneMapper,
            _platform = platform,
            _site = site,
            _licenseService = licenseService,
            _logStore = logStore
        );

    private readonly IAppStates _appStates;
    private readonly SystemFingerprint _fingerprint;
    private readonly IZoneMapper _zoneMapper;
    private readonly IPlatformInfo _platform;
    private readonly ISite _site;
    private readonly LazySvc<ILicenseService> _licenseService;
    private readonly ILogStoreLive _logStore;

    public SystemInfoSetDto GetSystemInfo()
    {
        var l = Log.Fn<SystemInfoSetDto>($"{_site.Id}");

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
        var warnings = _logStore.Segments
            .Where(l => l.Key.StartsWith(prefix))
            .Select(l => l.Value.Count)
            .Sum();
        return warnings;
    }
}