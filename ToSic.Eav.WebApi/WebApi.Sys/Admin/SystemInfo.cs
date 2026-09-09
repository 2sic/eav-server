using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys.ZoneMapper;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.Sys;
using ToSic.Eav.WebApi.Sys.Dto;
using ToSic.Sys.Capabilities.Fingerprints;
using ToSic.Sys.Capabilities.Licenses;
using ToSic.Sys.Capabilities.Platform;
using ToSic.Sys.Code.InfoSystem;

namespace ToSic.Eav.WebApi.Sys.Admin;

[PrivateApi]
[VisualQuery(
    NiceName = "System Info",
    NameId = "6ed9998e-7087-41c5-a26e-207e07359531",
    NameIds = ["System.SystemInfo"],
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.Internal,
    UiHint = "System information about the current portal and environment"
)]
public class SystemInfo : CustomDataSource
{
    private readonly IAppsCatalog _appsCatalog;
    private readonly SystemFingerprint _fingerprint;
    private readonly IZoneMapper _zoneMapper;
    private readonly IPlatformInfo _platform;
    private readonly ISite _site;
    private readonly LazySvc<ILicenseService> _licenseService;
    private readonly ILogStoreLive _logStore;

    public SystemInfo(
        Dependencies services,
        IAppsCatalog appsCatalog,
        SystemFingerprint fingerprint,
        IZoneMapper zoneMapper,
        IPlatformInfo platform,
        ISite site,
        LazySvc<ILicenseService> licenseService,
        ILogStoreLive logStore)
        : base(services, logName: "Sxc.SysInfo", connect: [appsCatalog, fingerprint, zoneMapper, platform, site, licenseService, logStore])
    {
        _appsCatalog = appsCatalog;
        _fingerprint = fingerprint;
        _zoneMapper = zoneMapper;
        _platform = platform;
        _site = site;
        _licenseService = licenseService;
        _logStore = logStore;

        ProvideOutRaw(GetSite, name: "Site");

        ProvideOutRaw(GetSystem, name: "System");

        ProvideOutRaw(GetLicense, name: "License");

        ProvideOutRaw(GetMessages, name: "Messages");
    }

    private IEnumerable<SiteStatsRaw> GetSite()
    {
        var l = Log.Fn<IEnumerable<SiteStatsRaw>>($"{_site.Id}");
        var zoneId = _site.ZoneId;

        var entity = new SiteStatsRaw(
            SiteId: _site.Id,
            ZoneId: zoneId,
            Apps: _appsCatalog.Apps(zoneId).Count,
            Languages: _zoneMapper.CulturesWithState(_site).Count
        );

        return l.Return([entity], "1");
    }

    private IEnumerable<SystemInfoRaw> GetSystem()
    {
        var l = Log.Fn<IEnumerable<SystemInfoRaw>>();

        var sysInfo = new SystemInfoRaw(
            Fingerprint: _fingerprint.GetFingerprint(),
            EavVersion: EavSystemInfo.VersionString,
            Platform: _platform.Name,
            PlatformVersion: EavSystemInfo.VersionToNiceFormat(_platform.Version),
            Zones: _appsCatalog.Zones.Count
        );

        return l.Return([sysInfo], "1");
    }

    private IEnumerable<LicenseInfoRaw> GetLicense()
    {
        var l = Log.Fn<IEnumerable<LicenseInfoRaw>>();
        var licenses = _licenseService.Value;

        var owner = string.Join(", ", licenses.All
            .Where(l => l.IsEnabled)
            .Select(l => l.Owner)
            .Where(o => o.HasValue())
            .Distinct());

        var entity = new LicenseInfoRaw(
            Main: "none",
            Count: licenses.All.Count,
            Owner: owner
        );

        return l.Return([entity], "1");
    }

    private IEnumerable<MessagesRaw> GetMessages()
    {
        var l = Log.Fn<IEnumerable<MessagesRaw>>();

        var warningsObsolete = CountInsightsMessages(CodeInfoConstants.ObsoleteNameInHistory);
        var warningsOther = CountInsightsMessages(LogConstants.StoreWarningsPrefix) - warningsObsolete;

        var entity = new MessagesRaw
        {
            WarningsOther = warningsOther,
            WarningsObsolete = warningsObsolete,
        };

        return l.Return([entity], "1");
    }

    private int CountInsightsMessages(string prefix)
    {
        return _logStore.SegmentCounts()
            .Where(s => s.Key.StartsWith(prefix))
            .Select(s => s.Value)
            .Sum();
    }
}
