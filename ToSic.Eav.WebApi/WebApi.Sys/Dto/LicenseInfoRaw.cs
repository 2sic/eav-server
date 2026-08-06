using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "LicenseInfo",
    Guid = "7c70aa77-af5c-4a74-9d17-e977cb93b80b",
    Description = "License information",
    Scope = "System"
)]
public class LicenseInfoRaw : IRawEntityAutoConvert
{
    [ContentTypeField(IsTitle = true)]
    public required string Main { get; init; }

    public required int Count { get; init; }

    public required string Owner { get; init; }
}
