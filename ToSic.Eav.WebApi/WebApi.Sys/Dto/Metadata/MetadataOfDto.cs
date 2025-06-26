using ToSic.Eav.ImportExport.Json.V1;

namespace ToSic.Eav.WebApi.Sys.Dto;

public class MetadataOfDto
{
    public required int Id { get; init; }

    public required Guid Guid { get; init; }
    public required JsonType Type { get; init; }


}