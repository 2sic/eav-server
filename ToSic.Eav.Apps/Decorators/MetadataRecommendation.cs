﻿using System;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Data;
using ToSic.Eav.Plumbing;

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

        public string Id => Type.NameId;

        public string Title { get; }

        public string Name => Type.Name;

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

        [JsonIgnore]
        public IContentType Type { get; }

        public MetadataRecommendation(IContentType type, IEntity recommendation, int? count, string debugMessage, int priority)
        {
            Type = type;
            //Id = type.NameId;
            //Name = type.Name;
            Priority = priority;
            var typeDetails = type.Metadata.DetailsOrNull;
            Title = (typeDetails?.Title).UseFallbackIfNoValue(type.Name);
            Icon = typeDetails?.Icon;
            var recDec = new ForDecorator(recommendation);
            Count = count ?? recDec.Amount;
            Debug = debugMessage;
            DeleteWarning = recDec.DeleteWarning;

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