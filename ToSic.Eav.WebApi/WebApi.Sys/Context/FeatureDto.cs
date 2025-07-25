﻿using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.WebApi.Sys.Context;

public class FeatureDto(FeatureState state, bool forSystemTypes)
{
    /// <summary>
    /// The static name of the feature, which is used in the UI to verify if it's enabled or not.
    /// </summary>
    [JsonPropertyName("nameId")]
    public string NameId => state.NameId;

    /// <summary>
    /// This is the real setting - if it's enabled.
    /// </summary>
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled => state.IsEnabled;

    /// <summary>
    /// This is the information for the UI, as it may be allowed to be used even if it's not enabled.
    /// It's mainly for system types, which are always allowed to use certain features such as picker features. 
    /// </summary>
    [JsonPropertyName("allowUse")]
    public bool AllowUse => state.IsEnabled || (forSystemTypes && state.Aspect.EnableForSystemTypes);

    /// <summary>
    /// Nicer name for the feature, which is used in the UI to show what it is.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name => state.Aspect.Name;

    /// <summary>
    /// What to do when the feature is disabled.
    /// ATM not in use yet as of v18.01
    /// </summary>
    [JsonPropertyName("behavior")]
    public string Behavior => state.Aspect.DisabledBehavior.ToString().ToLower();
}