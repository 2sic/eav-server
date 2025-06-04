using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Metadata.Recommendations.Sys;

namespace ToSic.Eav.WebApi.Admin.Metadata;

public class MetadataListDto
{
    public IEnumerable<MetadataRecommendation> Recommendations { get; set; }

    public IEnumerable<IDictionary<string, object>> Items { get; set; }

    public JsonMetadataFor For { get; set; }
}