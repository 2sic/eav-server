namespace ToSic.Eav.WebApi.Admin.Features;

public class FeaturesDto
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("msg")]
    public Msg Msg { get; set; }

}
public class Msg
{
    [JsonPropertyName("features")]
    public string Features { get; set; }

    [JsonPropertyName("signature")]
    public string Signature { get; set; }
}