using ToSic.Eav.WebApi.Sys.Admin.Metadata;

namespace ToSic.Eav.WebApi.Sys.Dto;

public class AppInternalsDto
{
    public required IDictionary<string, IEnumerable<IDictionary<string, object>>?> EntityLists { get; init; }
    public required IDictionary<string, IEnumerable<ContentTypeFieldDto>?> FieldAll { get; init; }
    public required MetadataListDto MetadataList { get; init; }
}