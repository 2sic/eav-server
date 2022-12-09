using System.Collections.Generic;
using System.Text.Json.Serialization;
using ToSic.Eav.Json.V1;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonFormat: JsonContentTypeSet
    {
        /// <summary>
        /// V1 - header information
        /// </summary>
        public JsonHeader _ = new JsonHeader();
        
        /// <summary>
        /// V1 - a single Entity
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        public JsonEntity Entity;

        ///// <summary>
        ///// V1 - a single Content-Type
        ///// </summary>
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        //public JsonContentType ContentType;


        ///// <summary>
        ///// V1.2 - A list of entities - added in 2sxc 12 to support content-types with additional sub-entities like formulas
        ///// </summary>
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        //public IEnumerable<JsonEntity> Entities;


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<JsonBundle> Bundles { get; set; }

    }
}
