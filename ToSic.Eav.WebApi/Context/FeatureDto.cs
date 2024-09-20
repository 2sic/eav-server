using ToSic.Eav.SysData;

namespace ToSic.Eav.WebApi.Context;

public class FeatureDto(FeatureState state, bool forSystemTypes)
{
    [JsonPropertyName("nameId")]
    public string NameId => state.NameId;

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled => state.IsEnabled;

    [JsonPropertyName("allowUse")]
    public bool AllowUse => state.IsEnabled || (forSystemTypes && state.Aspect.EnableForSystemTypes);

    [JsonPropertyName("name")]
    public string Name => state.Aspect.Name;

    [JsonPropertyName("behavior")]
    public string Behavior => state.Aspect.DisabledBehavior.ToString().ToLower();
}