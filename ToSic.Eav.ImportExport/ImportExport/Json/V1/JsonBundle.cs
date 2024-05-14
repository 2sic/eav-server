namespace ToSic.Eav.ImportExport.Json.V1;

/// <summary>
/// WIP new in v15
///
/// Should contain a set of things to preserve together
/// </summary>
public class JsonBundle
{
    public string Name { get; set; } = "default";

    [JsonIgnore(Condition = WhenWritingDefault)]
    public List<JsonContentTypeSet> ContentTypes { get; set; }

    [JsonIgnore(Condition = WhenWritingDefault)]
    public List<JsonEntity> Entities { get; set; }
}