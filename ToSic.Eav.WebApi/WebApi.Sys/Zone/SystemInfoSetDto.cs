namespace ToSic.Eav.WebApi.Sys.Zone;

public class SystemInfoSetDto
{
    public required SiteStatsDto Site { get; init; }

    public required SystemInfoDto System { get; init; }

    public required LicenseInfoDto License { get; init; }

    public required MessagesDto Messages { get; init; }
}

public class MessagesDto
{
    public required int WarningsOther { get; init; }
    public required int WarningsObsolete { get; init; }
    // public required int Errors { get; init; }
}

public class SystemInfoDto
{
    public required string Fingerprint { get; init; }

    public required string EavVersion { get; init; }

    public required string Platform { get; init; }

    public required string PlatformVersion { get; init; }

    public required int Zones { get; init; }
}

public class LicenseInfoDto
{
    public required string Main { get; init; }

    public required int Count { get; init; }

    public required string Owner { get; init; }
}
    

public class SiteStatsDto
{
    public required int SiteId { get; init; }

    public int? TenantId { get; init; }

    public required int ZoneId { get; init; }

    public required int Apps { get; init; }

    public required int Languages { get; init; }
}