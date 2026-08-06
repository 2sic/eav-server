using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "SystemInfo",
    Guid = "6ed9998e-7087-41c5-a26e-207e07359531",
    Description = "System information",
    Scope = "System"
)]
public class SystemInfoRaw : IRawEntityAutoConvert
{
    public required string Fingerprint { get; init; }

    public required string EavVersion { get; init; }

    [ContentTypeField(IsTitle = true)]
    public required string Platform { get; init; }

    public required string PlatformVersion { get; init; }

    public required int Zones { get; init; }
}
