using ToSic.Eav.WebApi.Sys.Admin.Metadata;

namespace ToSic.Eav.WebApi.Sys.Dto;

public class AppInternalsDto
{
    public IDictionary<string, IEnumerable<IDictionary<string, object>>> EntityLists { get; set; }
    public IDictionary<string, IEnumerable<ContentTypeFieldDto>> FieldAll { get; set; }
    public MetadataListDto MetadataList { get; set; }
}