using ToSic.Eav.Data.ContentTypes;

namespace ToSic.Eav.Apps.Inspect.Sys.WebApi.Models;

[ContentType(
    Name = "ApiFile",
    Guid = "e8ec1cb4-7068-4e09-bf03-0ead60ff3511",
    Description = "App WebApi controller file",
    Scope = "System"
)]
public record AppWebApiFileRaw : IRawEntityAutoConvert
{
    public required int id { get; init; }

    [ContentTypeTitle]
    public required string path { get; init; }

    public required string endpointPath { get; init; }
    public required string edition { get; init; }
    public required bool shared { get; init; }
}
