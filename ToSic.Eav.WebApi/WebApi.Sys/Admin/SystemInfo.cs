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
using ToSic.Sys.Logging;
using ToSic.Sys.Utils;

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
    private readonly IAppsCatalog appsCatalog;
    private readonly SystemFingerprint fingerprint;
    private readonly IZoneMapper zoneMapper;
    private readonly IPlatformInfo platform;
    private readonly ISite site;
    private readonly LazySvc<ILicenseService> licenseService;
    private readonly ILogStoreLive logStore;

    private SystemInfoSetDto? systemInfo;
    private SystemInfoSetDto SystemInfoSet => systemInfo ??= GetSystemInfo();

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
        this.appsCatalog = appsCatalog;
        this.fingerprint = fingerprint;
        this.zoneMapper = zoneMapper;
        this.platform = platform;
        this.site = site;
        this.licenseService = licenseService;
        this.logStore = logStore;

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
        var l = Log.Fn<IEnumerable<IRawEntity>>();
        var siteStats = SystemInfoSet.Site;

        var entity = new RawEntity(new()
        {
            { nameof(siteStats.SiteId), siteStats.SiteId },
            { nameof(siteStats.ZoneId), siteStats.ZoneId },
            { nameof(siteStats.Apps), siteStats.Apps },
            { nameof(siteStats.Languages), siteStats.Languages },
        });

        return l.Return([entity], "1");
    }

    private IEnumerable<IRawEntity> GetSystem()
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();
        var system = SystemInfoSet.System;

        var entity = new RawEntity(new()
        {
            { nameof(system.Fingerprint), system.Fingerprint },
            { nameof(system.EavVersion), system.EavVersion },
            { nameof(system.Platform), system.Platform },
            { nameof(system.PlatformVersion), system.PlatformVersion },
            { nameof(system.Zones), system.Zones },
        });

        return l.Return([entity], "1");
    }

    private IEnumerable<IRawEntity> GetLicense()
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();
        var license = SystemInfoSet.License;

        var entity = new RawEntity(new()
        {
            { nameof(license.Main), license.Main },
            { nameof(license.Count), license.Count },
            { nameof(license.Owner), license.Owner },
        });

        return l.Return([entity], "1");
    }

    private IEnumerable<IRawEntity> GetMessages()
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();
        var messages = SystemInfoSet.Messages;

        var entity = new RawEntity(new()
        {
            { nameof(messages.WarningsOther), messages.WarningsOther },
            { nameof(messages.WarningsObsolete), messages.WarningsObsolete },
        });

        return l.Return([entity], "1");
    }

    private SystemInfoSetDto GetSystemInfo()
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
            PlatformVersion = EavSystemInfo.VersionToNiceFormat(platform.Version),
        };

        var licenses = licenseService.Value;

        var owner = string.Join(", ", licenses.All
            .Where(l => l.IsEnabled)
            .Select(l => l.Owner)
            .Where(o => o.HasValue())
            .Distinct());

        var license = new LicenseInfoDto
        {
            Count = licenses.All.Count,
            Main = "none",
            Owner = owner,
        };

        var warningsObsolete = CountInsightsMessages(CodeInfoConstants.ObsoleteNameInHistory);
        var warningsOther = CountInsightsMessages(LogConstants.StoreWarningsPrefix) - warningsObsolete;

        var messages = new MessagesDto
        {
            WarningsObsolete = warningsObsolete,
            WarningsOther = warningsOther,
        };

        var info = new SystemInfoSetDto
        {
            License = license,
            Site = siteStats,
            System = sysInfo,
            Messages = messages,
        };

        return l.ReturnAsOk(info);
    }

    private int CountInsightsMessages(string prefix)
    {
        return logStore.Segments
            .Where(s => s.Key.StartsWith(prefix))
            .Select(s => s.Value.Count)
            .Sum();
    }
}