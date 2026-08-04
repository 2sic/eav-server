using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.WebApi.Sys.Zone;

namespace ToSic.Eav.WebApi.Sys.Admin;

[ContentTypeSpecs(
    Name = "SiteStats", 
    Guid = "f59a06bb-60ad-4d65-8c02-799d46f5e640", 
    Description = "Site statistics", 
    Scope = "System"
    )]
public class SiteStatsModel(SiteStatsDto site) : RawEntity
{
    [ContentTypeAttributeSpecs(IsTitle = true)] public int SiteId => site.SiteId;
    public override IDictionary<string, object?> Attributes(RawConvertOptions options) => new Dictionary<string, object?>
    {
        { nameof(SiteStatsDto.SiteId), site.SiteId }, { nameof(SiteStatsDto.ZoneId), site.ZoneId },
        { nameof(SiteStatsDto.Apps), site.Apps }, { nameof(SiteStatsDto.Languages), site.Languages },
    };
}

[ContentTypeSpecs(
    Name = "SystemInfo", 
    Guid = "6ed9998e-7087-41c5-a26e-207e07359531", 
    Description = "System information", 
    Scope = "System"
    )]
public class SystemInfoModel(SystemInfoDto system) : RawEntity
{
    [ContentTypeAttributeSpecs(IsTitle = true)] public string Platform => system.Platform;
    public override IDictionary<string, object?> Attributes(RawConvertOptions options) => new Dictionary<string, object?>
    {
        { nameof(SystemInfoDto.Fingerprint), system.Fingerprint }, { nameof(SystemInfoDto.EavVersion), system.EavVersion },
        { nameof(SystemInfoDto.Platform), system.Platform }, { nameof(SystemInfoDto.PlatformVersion), system.PlatformVersion },
        { nameof(SystemInfoDto.Zones), system.Zones },
    };
}

[ContentTypeSpecs(
    Name = "LicenseInfo", 
    Guid = "7c70aa77-af5c-4a74-9d17-e977cb93b80b", 
    Description = "License information", 
    Scope = "System"
    )]
public class LicenseInfoModel(LicenseInfoDto license) : RawEntity
{
    [ContentTypeAttributeSpecs(IsTitle = true)] public string Main => license.Main;
    public override IDictionary<string, object?> Attributes(RawConvertOptions options) => new Dictionary<string, object?>
    {
        { nameof(LicenseInfoDto.Main), license.Main }, { nameof(LicenseInfoDto.Count), license.Count }, { nameof(LicenseInfoDto.Owner), license.Owner },
    };
}

[ContentTypeSpecs(
    Name = "Messages", 
    Guid = "41bc9f69-6760-4cab-9004-6c848ed2e569", 
    Description = "System message statistics", 
    Scope = "System"
    )]
public class MessagesModel(MessagesDto messages) : RawEntity
{
    public override IDictionary<string, object?> Attributes(RawConvertOptions options) => new Dictionary<string, object?>
    {
        { nameof(MessagesDto.WarningsOther), messages.WarningsOther }, { nameof(MessagesDto.WarningsObsolete), messages.WarningsObsolete },
    };
}