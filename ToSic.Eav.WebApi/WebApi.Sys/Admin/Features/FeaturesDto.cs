namespace ToSic.Eav.WebApi.Sys.Admin.Features;

public class FeaturesDto
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("msg")]
    public required Msg Msg { get; init; }

}
public class Msg
{
    [JsonPropertyName("features")]
    public required string Features { get; init; }

    [JsonPropertyName("signature")]
    public required string Signature { get; init; }
}