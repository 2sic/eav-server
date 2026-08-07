using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "ApiFile",
    Guid = "98e35962-ae3c-44b3-a3fd-1275419825c7",
    Description = "App WebApi controller file",
    Scope = "System"
)]
public record AppWebApiFileRaw : IRawEntityAutoConvert
{
    public required int id { get; init; }

    [ContentTypeField(IsTitle = true)]
    public required string path { get; init; }

    public required string endpointPath { get; init; }
    public required string edition { get; init; }
    public required bool shared { get; init; }
}
