using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "SiteStats",
    Guid = "f59a06bb-60ad-4d65-8c02-799d46f5e640",
    Description = "Site statistics",
    Scope = "System"
)]
public class SiteStatsRaw : IRawEntityAutoConvert
{
    [ContentTypeField(IsTitle = true)]
    public required int SiteId { get; init; }

    public required int ZoneId { get; init; }

    public required int Apps { get; init; }

    public required int Languages { get; init; }
}
