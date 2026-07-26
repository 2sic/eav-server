using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys.ZoneMapper;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.Sys;
using ToSic.Eav.WebApi.Sys.Zone;
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

        ProvideOutRaw(GetSite, name: "Site", options: () => new()
        {
            TitleField = nameof(SiteStatsDto.SiteId),
            TypeName = "SiteStats",
        });

        ProvideOutRaw(GetSystem, name: "System", options: () => new()
        {
            TitleField = nameof(SystemInfoDto.Platform),
            TypeName = "SystemInfo",
        });

        ProvideOutRaw(GetLicense, name: "License", options: () => new()
        {
            TitleField = nameof(LicenseInfoDto.Main),
            TypeName = "LicenseInfo",
        });

        ProvideOutRaw(GetMessages, name: "Messages", options: () => new()
        {
            TypeName = "Messages",
        });
    }

    private IEnumerable<IRawEntity> GetSite()
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>($"{_site.Id}");
        var zoneId = _site.ZoneId;

        var entity = new RawEntity
        {
            Values = new Dictionary<string, object?>
            {
                { nameof(SiteStatsDto.SiteId), _site.Id },
                { nameof(SiteStatsDto.ZoneId), zoneId },
                { nameof(SiteStatsDto.Apps), _appsCatalog.Apps(zoneId).Count },
                { nameof(SiteStatsDto.Languages), _zoneMapper.CulturesWithState(_site).Count },
            }
        };

        return l.Return([entity], "1");
    }

    private IEnumerable<IRawEntity> GetSystem()
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();

        var entity = new RawEntity
        {
            Values = new Dictionary<string, object?>
            {
                { nameof(SystemInfoDto.Fingerprint), _fingerprint.GetFingerprint() },
                { nameof(SystemInfoDto.EavVersion), EavSystemInfo.VersionString },
                { nameof(SystemInfoDto.Platform), _platform.Name },
                { nameof(SystemInfoDto.PlatformVersion), EavSystemInfo.VersionToNiceFormat(_platform.Version) },
                { nameof(SystemInfoDto.Zones), _appsCatalog.Zones.Count },
            }
        };

        return l.Return([entity], "1");
    }

    private IEnumerable<IRawEntity> GetLicense()
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();
        var licenses = _licenseService.Value;

        var owner = string.Join(", ", licenses.All
            .Where(l => l.IsEnabled)
            .Select(l => l.Owner)
            .Where(o => o.HasValue())
            .Distinct());

        var entity = new RawEntity
        {
            Values = new Dictionary<string, object?>
            {
                { nameof(LicenseInfoDto.Main), "none" },
                { nameof(LicenseInfoDto.Count), licenses.All.Count },
                { nameof(LicenseInfoDto.Owner), owner },
            }
        };

        return l.Return([entity], "1");
    }

    private IEnumerable<IRawEntity> GetMessages()
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();

        var warningsObsolete = CountInsightsMessages(CodeInfoConstants.ObsoleteNameInHistory);
        var warningsOther = CountInsightsMessages(LogConstants.StoreWarningsPrefix) - warningsObsolete;

        var entity = new RawEntity
        {
            Values = new Dictionary<string, object?>
            {
                { nameof(MessagesDto.WarningsOther), warningsOther },
                { nameof(MessagesDto.WarningsObsolete), warningsObsolete },
            }
        };

        return l.Return([entity], "1");
    }

    private int CountInsightsMessages(string prefix)
    {
        return _logStore.Segments
            .Where(s => s.Key.StartsWith(prefix))
            .Select(s => s.Value.Count)
            .Sum();
    }
}