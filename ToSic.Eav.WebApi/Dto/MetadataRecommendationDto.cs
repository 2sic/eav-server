using System;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Data;
using ToSic.Eav.Types;

namespace ToSic.Eav.WebApi.Dto
{
    public class MetadataRecommendationDto: IEquatable<MetadataRecommendationDto>
    {
        public string Id { get; }

        public string Name { get; }

        public int Count { get; set; }

        /// <summary>
        /// Marks the recommendation that it should be created as empty
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? CreateEmpty { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Debug { get; set; }

        public MetadataRecommendationDto(IContentType type, int count, string debugMessage)
        {
            Id = type.StaticName;
            Name = type.Name;
            Count = count;
            Debug = debugMessage;

            // Mark empty if possible
            if (!type.Attributes.Any() && type.Metadata.HasType(Decorators.SaveEmptyDecoratorId))
                CreateEmpty = true;
        }

        #region Equality Comparison for deduplication
        public bool Equals(MetadataRecommendationDto other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MetadataRecommendationDto)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }
        #endregion
    }
}
