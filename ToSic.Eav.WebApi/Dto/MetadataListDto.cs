using System.Collections.Generic;
using ToSic.Eav.Apps.AppMetadata;
using ToSic.Eav.ImportExport.Json.V1;

namespace ToSic.Eav.WebApi.Dto
{
    public class MetadataListDto
    {
        public IEnumerable<MetadataRecommendation> Recommendations { get; set; }

        public IEnumerable<IDictionary<string, object>> Items { get; set; }

        public JsonMetadataFor For { get; set; }
    }
}
