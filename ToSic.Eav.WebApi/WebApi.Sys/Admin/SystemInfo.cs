
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
    DataConfidentiality = DataConfidentiality.Confidential,
    UiHint = "System information about the current portal and environment"
)]

public class SystemInfo : CustomDataSource
{
    public SystemInfo(Dependencies services, ZoneBackend zoneBackend)
        : base(services, logName: "Sxc.SysInfo", connect: [zoneBackend])
    {
        ProvideOutRaw(
            () => GetSite(zoneBackend),
            name: "Site",
            options: () => new()
            {
                AutoId = true,
                TitleField = nameof(SiteStatsDto.SiteId),
                TypeName = "SiteStats",
            });

        ProvideOutRaw(
            () => GetSystem(zoneBackend),
            name: "System",
            options: () => new()
            {
                AutoId = true,
                TitleField = nameof(SystemInfoDto.Platform),
                TypeName = "SystemInfo",
            });

        ProvideOutRaw(
            () => GetLicense(zoneBackend),
            name: "License",
            options: () => new()
            {
                AutoId = true,
                TitleField = nameof(LicenseInfoDto.Main),
                TypeName = "LicenseInfo",
            });

        ProvideOutRaw(
            () => GetMessages(zoneBackend),
            name: "Messages",
            options: () => new()
            {
                AutoId = true,
                TypeName = "Messages",
            });
    }

    private IEnumerable<IRawEntity> GetSite(ZoneBackend zoneBackend)
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();
        var info = zoneBackend.GetSystemInfo();
        var site = info.Site;

        var entity = new RawEntity(new()
        {
            { nameof(site.SiteId), site.SiteId },
            { nameof(site.ZoneId), site.ZoneId },
            { nameof(site.Apps), site.Apps },
            { nameof(site.Languages), site.Languages },
        });

        return l.Return([entity], "1");
    }

    private IEnumerable<IRawEntity> GetSystem(ZoneBackend zoneBackend)
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();
        var info = zoneBackend.GetSystemInfo();
        var system = info.System;

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

    private IEnumerable<IRawEntity> GetLicense(ZoneBackend zoneBackend)
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();
        var info = zoneBackend.GetSystemInfo();
        var license = info.License;

        var entity = new RawEntity(new()
        {
            { nameof(license.Main), license.Main },
            { nameof(license.Count), license.Count },
            { nameof(license.Owner), license.Owner },
        });

        return l.Return([entity], "1");
    }

    private IEnumerable<IRawEntity> GetMessages(ZoneBackend zoneBackend)
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();
        var info = zoneBackend.GetSystemInfo();
        var messages = info.Messages;

        var entity = new RawEntity(new()
        {
            { nameof(messages.WarningsOther), messages.WarningsOther },
            { nameof(messages.WarningsObsolete), messages.WarningsObsolete },
        });

        return l.Return([entity], "1");
    }
}