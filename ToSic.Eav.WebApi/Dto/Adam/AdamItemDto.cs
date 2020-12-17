using System;
using Newtonsoft.Json;

namespace ToSic.Eav.WebApi.Dto
{
    public class AdamItemDto
    {
        public bool IsFolder { get; }
        public bool AllowEdit { get; set; }
        public int Size { get; set; }
        public int MetadataId { get; set; }

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
