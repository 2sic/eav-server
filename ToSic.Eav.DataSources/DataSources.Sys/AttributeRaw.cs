using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.DataSources.Sys;

[ContentType(
    Guid = "bdc80e35-4e30-43bb-a0b4-d031f07b5c41",
    Description = "Attribute/field information",
    Name = "Attribute"
)]
internal sealed record AttributeRaw(
    string Name,
    string Type,
    bool IsTitle,
    int SortOrder,
    bool IsBuiltIn,
    [property: ContentTypeTitle] string Title,
    string ContentType,
    string? Description
) : IRawEntityAutoConvert;