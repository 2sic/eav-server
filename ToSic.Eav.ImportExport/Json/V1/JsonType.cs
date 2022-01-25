using Newtonsoft.Json;
using ToSic.Eav.Data;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonType
    {
        public string Name;
        public string Id;

        /// <summary>
        /// Additional description for the type, usually not included.
        /// Sometimes added in admin-UI scenarios, where additional info is useful
        /// ATM only used for Metadata
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        /// <summary>
        /// Empty constructor is important for de-serializing
        /// </summary>
        public JsonType() { }

        public JsonType(IEntity entity, bool withDescription = false)
        {
            Name = entity.Type.Name;
            Id = entity.Type.NameId;
            if (withDescription) Description = entity.Type.Description;
        }
    }
}
