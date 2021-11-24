using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Data;
using ToSic.Eav.Types;

namespace ToSic.Eav.WebApi.Dto
{
    public class MetadataRecommendationDto
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int Count { get; set; }

        /// <summary>
        /// Marks the recommendation that it should be created as empty
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? CreateEmpty { get; set; }

        public MetadataRecommendationDto(IContentType type, int count)
        {
            Id = type.StaticName;
            Name = type.Name;
            Count = count;

            // Mark empty if possible
            if (!type.Attributes.Any() && type.Metadata.HasType(Decorators.SaveEmptyDecoratorId))
                CreateEmpty = true;
        }
    }
}
