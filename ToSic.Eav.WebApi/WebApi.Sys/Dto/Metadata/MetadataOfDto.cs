using ToSic.Eav.ImportExport.Json.V1;

namespace ToSic.Eav.WebApi.Sys.Dto;

public class MetadataOfDto
{
    public int Id { get; set; }

    public Guid Guid { get; set; }
    public JsonType Type { get; set; }


}