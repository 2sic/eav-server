using System.Collections.Generic;
using ToSic.Eav.ImportExport.Json.V1;

namespace ToSic.Eav.WebApi.Dto
{
    public class MetadataListDto
    {
        public IEnumerable<MetadataRecommendationDto> Recommendations { get; set; }

        public IEnumerable<IDictionary<string, object>> Items { get; set; }

        public JsonMetadataFor For { get; set; }
    }
}
