using static System.Text.Json.Serialization.JsonIgnoreCondition;

namespace ToSic.Eav.ImportExport.Json.V1;

public class JsonContentTypeShareable
{
    [JsonIgnore(Condition = WhenWritingDefault)] public bool AlwaysShare;
    [JsonIgnore(Condition = WhenWritingDefault)] public int ParentZoneId;
    [JsonIgnore(Condition = WhenWritingDefault)] public int ParentAppId;
    [JsonIgnore(Condition = WhenWritingDefault)] public int? ParentId;
}