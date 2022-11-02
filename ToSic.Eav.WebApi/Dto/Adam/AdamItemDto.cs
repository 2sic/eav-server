using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ToSic.Eav.WebApi.Dto.Metadata;

namespace ToSic.Eav.WebApi.Dto
{
    /// <summary>
    /// WIP base class until the UploadResultDto and AdamItemDto are merged - there should not be so many classes
    /// </summary>
    public class AdamItemDtoBase
    {
        /// <summary>
        /// Optional error message, should normally be null if no error
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Error { get; set; }

        /// <summary>
        /// The file name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// WIP: this should contain the code like "file:2742"
        /// </summary>
        public string ReferenceId { get; set; }

        /// <summary>
        /// Normal url to access the resource
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Url { get; set; }

        /// <summary>
        /// The Adam type, such as "folder", "image" etc.
        /// </summary>
        public string Type { get; set; }
    }

    public class AdamItemDto: AdamItemDtoBase
    {
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

        public DateTime Created { get; }
        public DateTime Modified { get; }

        /// <summary>
        /// Small preview thumbnail
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// Large preview
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string PreviewUrl { get; set; }

        public AdamItemDto(string error)
        {
            Error = error;
        }

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
