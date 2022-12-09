﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonContentTypeSet
    {
        /// <summary>
        /// V1 - header information
        /// </summary>
        public JsonHeader _ = new JsonHeader();

        /// <summary>
        /// V1 - a single Content-Type
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        public JsonContentType ContentType;


        /// <summary>
        /// V1.2 - A list of entities - added in 2sxc 12 to support content-types with additional sub-entities like formulas
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public IEnumerable<JsonEntity> Entities;

    }
}