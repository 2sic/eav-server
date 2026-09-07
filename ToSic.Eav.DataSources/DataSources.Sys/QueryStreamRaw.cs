using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.DataSources.Sys;

[ContentType(
    Guid = "d1a476cc-9716-4c8b-a0d5-eaa70594f7d2",
    Description = "Output stream of a query",
    Name = "QueryStream"
)]
internal sealed record QueryStreamRaw(
    [property: ContentTypeTitle] string Name
) : IRawEntityAutoConvert;