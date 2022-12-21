using System.Text.Json.Serialization;
using ToSic.Eav.Data;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonType
    {
        public string Name;
        public string Id;

        // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
        /// <summary>
        /// Additional description for the type, usually not included.
        /// Sometimes added in admin-UI scenarios, where additional info is useful
        /// ATM only used for Metadata
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Description { get; set; }

        /// <summary>
        /// Additional description for the type, usually not included.
        /// Sometimes added in admin-UI scenarios, where additional info is useful
        /// ATM only used for Metadata
        /// </summary>
        /// <remarks>Added in v13.02</remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Title { get; set; }

        /// <summary>
        /// Empty constructor is important for de-serializing
        /// </summary>
        public JsonType() { }

        public JsonType(IEntity entity, bool withDescription = false)
        {
            Name = entity.Type.Name;
            Id = entity.Type.NameId;
            if (withDescription)
            {
                var description = entity.Type.Metadata.DetailsOrNull;
                Title = description?.Title ?? entity.Type.NameId;
                // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
                Description = description?.Description ?? ""; // entity.Type.Description;
            }
        }
    }
}
