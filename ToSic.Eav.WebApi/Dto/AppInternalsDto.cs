using ToSic.Eav.WebApi.Admin.Metadata;

namespace ToSic.Eav.WebApi.Dto;

public class AppInternalsDto
{
    public IDictionary<string, IEnumerable<IDictionary<string, object>>> EntityLists { get; set; }
    public IDictionary<string, IEnumerable<ContentTypeFieldDto>> FieldAll { get; set; }
    public MetadataListDto MetadataList { get; set; }
}