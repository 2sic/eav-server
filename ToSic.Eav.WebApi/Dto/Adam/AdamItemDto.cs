using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ToSic.Eav.WebApi.Dto.Metadata;

namespace ToSic.Eav.WebApi.Dto
{
    public class AdamItemDto
    {
        /// <summary>
        /// WIP: this should contain the code like "file:2742"
        /// </summary>
        public string ReferenceId { get; set; }

        public bool IsFolder { get; }
        public bool AllowEdit { get; set; }
        public int Size { get; set; }

        /// <summary>
        /// TEMP / WIP - should contain the ID of the entity which provides metadata
        /// of course it would only work with one entity, so it's not a final design choice
        /// </summary>
        public int MetadataId { get; set; }
        
        /// <summary>
        /// WIP 13.05
        /// </summary>
        public IEnumerable<MetadataOfDto> Metadata { get; set; }

        public string Path { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public DateTime Created { get; }
        public DateTime Modified { get; }

        /// <summary>
        /// Small preview thumbnail
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// Large preview
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PreviewUrl { get; set; }

        /// <summary>
        /// Normal url to access the resource
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        public AdamItemDto(bool isFolder, string name, int size, DateTime created, DateTime modified)
        {
            IsFolder = isFolder;
            // note that the type will be set by other code later on if it's a file
            Type = isFolder ? "folder" : "unknown";
            Name = name;
            Size = size;
            Created = created;
            Modified = modified;
        }

    }
}
