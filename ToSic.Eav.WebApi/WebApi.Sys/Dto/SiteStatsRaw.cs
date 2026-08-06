using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "SiteStats",
    Guid = "f59a06bb-60ad-4d65-8c02-799d46f5e640",
    Description = "Site statistics",
    Scope = "System"
)]
public record SiteStatsRaw(
    [property: ContentTypeField(IsTitle = true)]
    int SiteId,
    int ZoneId,
    int Apps,
    int Languages
) : IRawEntityAutoConvert;
