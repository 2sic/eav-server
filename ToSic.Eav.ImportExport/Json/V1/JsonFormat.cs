using System.Collections.Generic;
using Newtonsoft.Json;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonFormat
    {
        /// <summary>
        /// V1 - header information
        /// </summary>
        public JsonHeader _ = new JsonHeader();
        
        /// <summary>
        /// V1 - a single Entity
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public JsonEntity Entity;
        
        /// <summary>
        /// V1 - a single Content-Type
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public JsonContentType ContentType;
        
        
        /// <summary>
        /// V1.2 - A list of entities - added in 2sxc 12 to support content-types with additional sub-entities like formulas
        /// </summary>
        /// <remarks>
        /// Will not be serialized if default value (empty array)
        /// </remarks>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)] 
        public IEnumerable<JsonEntity> Entities;
    }
}
