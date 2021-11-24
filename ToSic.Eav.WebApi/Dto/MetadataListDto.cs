using System.Collections.Generic;

namespace ToSic.Eav.WebApi.Dto
{
    public class MetadataListDto
    {
        public IEnumerable<MetadataRecommendationDto> Recommendations { get; set; }

        public IEnumerable<IDictionary<string, object>> Items { get; set; }
    }
}
