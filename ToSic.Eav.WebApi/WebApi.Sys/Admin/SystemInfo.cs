
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.WebApi.Sys.Zone;

namespace ToSic.Eav.WebApi.Sys.Admin;

[PrivateApi]
[VisualQuery(
    NiceName = "System Info",
    NameId = "6ed9998e-7087-41c5-a26e-207e07359531",
    NameIds = ["System.SystemInfo"], // Internal name for the system, used in some entity-pickers. Can change at any time.
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.Internal,
    UiHint = "System information about the current portal and environment"
)]
public class SystemInfo : CustomDataSource
{
    private readonly ZoneBackend zoneBackend;
    private SystemInfoSetDto? systemInfo;

    private SystemInfoSetDto SystemInfoSet
        => systemInfo ??= zoneBackend.GetSystemInfo();

    public SystemInfo(Dependencies services, ZoneBackend zoneBackend)
        : base(services, logName: "Sxc.SysInfo", connect: [zoneBackend])
    {
        this.zoneBackend = zoneBackend;

        ProvideOutRaw(
            GetSite,
            name: "Site",
            options: () => new()
            {
                TitleField = nameof(SiteStatsDto.SiteId),
                TypeName = "SiteStats",
            });

        ProvideOutRaw(
            GetSystem,
            name: "System",
            options: () => new()
            {
                TitleField = nameof(SystemInfoDto.Platform),
                TypeName = "SystemInfo",
            });

        ProvideOutRaw(
            GetLicense,
            name: "License",
            options: () => new()
            {
                TitleField = nameof(LicenseInfoDto.Main),
                TypeName = "LicenseInfo",
            });

        ProvideOutRaw(
            GetMessages,
            name: "Messages",
            options: () => new()
            {
                TypeName = "Messages",
            });
    }

    private IEnumerable<IRawEntity> GetSite()
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();
        var site = SystemInfoSet.Site;

        var entity = new RawEntity(new()
        {
            { nameof(site.SiteId), site.SiteId },
            { nameof(site.ZoneId), site.ZoneId },
            { nameof(site.Apps), site.Apps },
            { nameof(site.Languages), site.Languages },
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

    // TODO: @2rb - here you will put the code from the ZoneBackend, since it will be more complex
    private SystemInfoSetDto GetSystemInfo()
    {
        return _zoneBackend.GetSystemInfo();
    }

}