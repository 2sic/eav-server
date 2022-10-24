using System.Text.Json.Serialization;

namespace ToSic.Eav.ImportExport.Json.V1
{
    public class JsonContentTypeShareable
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public bool AlwaysShare;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public int ParentZoneId;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public int ParentAppId;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public int? ParentId;
    }
}
