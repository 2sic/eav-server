using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.DataSources.Sys;

[ContentType(
    Name = "Zone",
    Guid = "b23e155d-082a-4343-a799-409df19c081c"
)]
internal sealed record ZoneRaw(
    int Id,
    [property: ContentTypeTitle] string Name,
    int? TenantId,
    string? TenantName,
    int DefaultAppId,
    int PrimaryAppId,
    bool IsCurrent,
    int AppCount
) : IRawEntityAutoConvert;
