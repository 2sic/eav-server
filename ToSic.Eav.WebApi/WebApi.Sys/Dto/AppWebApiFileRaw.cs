using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "ApiFile",
    Guid = "98e35962-ae3c-44b3-a3fd-1275419825c7",
    Description = "App WebApi controller file",
    Scope = "System"
)]
public class AppWebApiFileRaw : IRawEntityAutoConvert
{
    // TODO: @2rb - fix, should be init only
    public int Id { get; set; }

    [ContentTypeField(IsTitle = true)]
    public required string Path { get; init; }

    public required string EndpointPath { get; init; }
    public required string Edition { get; init; }
    public bool? Shared { get; init; }
}
