using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonAttributeDefinition
    {
        public string Name;
        public string Type;
        public string InputType;    // added 2019-09-02 for 2sxc 10.03 to enhance UI handling
        public bool IsTitle;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public List<JsonEntity> Metadata;
    }
}
