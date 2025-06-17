using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Metadata.Recommendations.Sys;

namespace ToSic.Eav.WebApi.Sys.Admin.Metadata;

public class MetadataListDto
{
    public required IEnumerable<MetadataRecommendation> Recommendations { get; init; }

    public required IEnumerable<IDictionary<string, object>> Items { get; init; }

    public required JsonMetadataFor For { get; init; }
}