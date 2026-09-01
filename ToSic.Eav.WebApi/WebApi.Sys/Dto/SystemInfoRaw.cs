using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "SystemInfo",
    Guid = "6ed9998e-7087-41c5-a26e-207e07359531",
    Description = "System information",
    Scope = "System"
)]
public record SystemInfoRaw(
    string Fingerprint,
    string EavVersion,
    [property: ContentTypeField(IsTitle = true)]
    string Platform,
    string PlatformVersion,
    int Zones
) : IRawEntityAutoConvert;  
