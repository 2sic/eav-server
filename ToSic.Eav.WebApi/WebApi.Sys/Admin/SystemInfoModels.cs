using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.WebApi.Sys.Zone;

namespace ToSic.Eav.WebApi.Sys.Admin;

[ContentType(
    Name = "SiteStats",
    Guid = "f59a06bb-60ad-4d65-8c02-799d46f5e640",
    Description = "Site statistics",
    Scope = "System"
)]
public record SiteStatsModel(SiteStatsDto site) : RawEntity
{
    [ContentTypeField(IsTitle = true)] public int SiteId => site.SiteId;
    protected override IDictionary<string, object?> GetValues() => new Dictionary<string, object?>
    {
        { nameof(SiteStatsDto.SiteId), site.SiteId }, { nameof(SiteStatsDto.ZoneId), site.ZoneId },
        { nameof(SiteStatsDto.Apps), site.Apps }, { nameof(SiteStatsDto.Languages), site.Languages },
    };
}

[ContentType(
    Name = "SystemInfo",
    Guid = "6ed9998e-7087-41c5-a26e-207e07359531",
    Description = "System information",
    Scope = "System"
)]
public record SystemInfoModel(SystemInfoDto system) : RawEntity
{
    [ContentTypeField(IsTitle = true)] public string Platform => system.Platform;
    protected override IDictionary<string, object?> GetValues() => new Dictionary<string, object?>
    {
        { nameof(SystemInfoDto.Fingerprint), system.Fingerprint }, { nameof(SystemInfoDto.EavVersion), system.EavVersion },
        { nameof(SystemInfoDto.Platform), system.Platform }, { nameof(SystemInfoDto.PlatformVersion), system.PlatformVersion },
        { nameof(SystemInfoDto.Zones), system.Zones },
    };
}

[ContentType(
    Name = "LicenseInfo",
    Guid = "7c70aa77-af5c-4a74-9d17-e977cb93b80b",
    Description = "License information",
    Scope = "System"
)]
public record LicenseInfoModel(LicenseInfoDto license) : RawEntity
{
    [ContentTypeField(IsTitle = true)] public string Main => license.Main;
    protected override IDictionary<string, object?> GetValues() => new Dictionary<string, object?>
    {
        { nameof(LicenseInfoDto.Main), license.Main }, { nameof(LicenseInfoDto.Count), license.Count }, { nameof(LicenseInfoDto.Owner), license.Owner },
    };
}

[ContentType(
    Name = "Messages",
    Guid = "41bc9f69-6760-4cab-9004-6c848ed2e569",
    Description = "System message statistics",
    Scope = "System"
)]
public record MessagesModel(MessagesDto messages) : RawEntity
{
    protected override IDictionary<string, object?> GetValues() => new Dictionary<string, object?>
    {
        { nameof(MessagesDto.WarningsOther), messages.WarningsOther }, { nameof(MessagesDto.WarningsObsolete), messages.WarningsObsolete },
    };
}