using ToSic.Eav.SysData;

namespace ToSic.Eav.WebApi.Context;

public class FeatureDto(FeatureState state)
{
    [JsonPropertyName("nameId")]
    public string NameId => state.NameId;

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled => state.IsEnabled;

    [JsonPropertyName("name")]
    public string Name => state.Aspect.Name;
}