using System;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Decorators
{
    /// <summary>
    /// Important: also used as DTO, so don't just rename the parameters
    /// </summary>
    public class MetadataRecommendation: IEquatable<MetadataRecommendation>
    {
        public const int PrioMax = 100;
        public const int PrioLow = 1;
        public const int PrioMedium = 2;
        public const int PrioHigh = 10;

        public string Id { get; }

        public string Title { get; }

        public string Name { get; }

        public int Count { get; set; }

        public string DeleteWarning { get; set; }

        public string Icon { get; set; }

        [JsonIgnore]
        public int Priority { get; set; }

        /// <summary>
        /// Marks the recommendation that it should be created as empty
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? CreateEmpty { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Debug { get; set; }

        public MetadataRecommendation(IContentType type, IEntity recommendation, int? count, string debugMessage, int priority)
        {
            Id = type.NameId;
            Name = type.Name;
            Priority = priority;
            var typeDescription = type.Metadata.Description;
            // Note: we cannot use GetBestTitle here, because the Content-type of the type is not really known
            // Because it's usually an inherited type (bug/weakness in the shared types model as of v12, WIP)
            // So we must use .Value
            Title = typeDescription?.Value<string>(ContentTypes.ContentTypeMetadataLabel) ?? type.Name;
            Icon = typeDescription?.Value<string>(ContentTypes.ContentTypeMetadataIcon) ?? type.Name;
            var recDec = new ForDecorator(recommendation);
            Count = count ?? recDec.Amount;
            Debug = debugMessage;
            DeleteWarning = recDec.DeleteWarning; // recommendation?.Value<string>(ForDecorator.DeleteWarningField);

            // Mark empty if possible - so it has no attributes, and it has a decorator to support this
            if (!type.Attributes.Any() && type.Metadata.HasType(Metadata.Decorators.SaveEmptyDecoratorId))
                CreateEmpty = true;
        }

        #region Equality Comparison for deduplication
        public bool Equals(MetadataRecommendation other)
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
            return Equals((MetadataRecommendation)obj);
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
