﻿using System;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Data;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.WebApi.Dto
{
    public class MetadataRecommendationDto: IEquatable<MetadataRecommendationDto>
    {
        public string Id { get; }

        public string Title { get; }

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
            // Note: we cannot use GetBestTitle here, because the Content-type of the type is not really known
            // Because it's usually an inherited type (bug/weakness in the shared types model as of v12, WIP)
            // So we must use .Value
            Title = type.Metadata.Description?.Value<string>(ContentTypes.ContentTypeMetadataLabel) ?? type.Name;
            Count = count;
            Debug = debugMessage;

            // Mark empty if possible - so it has no attributes, and it has a decorator to support this
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
