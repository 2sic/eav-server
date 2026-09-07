using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.DataSources.Sys;

[ContentType(
    Guid = "52c96a68-c791-439f-a154-7c46c1bfa0d9",
    Description = "Metadata target type",
    Name = "MetadataTargetTypes"
)]
internal sealed record MetadataTargetTypeRaw(
    int Id,
    [property: ContentTypeTitle] string Title,
    string NameId
) : IRawEntityAutoConvert;