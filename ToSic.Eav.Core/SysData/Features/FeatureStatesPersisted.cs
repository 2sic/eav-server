﻿using System.Text.Json.Serialization;

namespace ToSic.Eav.SysData;

[PrivateApi("no good reason to publish this")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class FeatureStatesPersisted
{
    [JsonPropertyName("features")]
    public List<FeatureStatePersisted> Features = [];

    /// <summary>
    /// Fingerprint must be included for ensuring integrity of the data
    /// </summary>
    [JsonPropertyName("fingerprint")]
    public string Fingerprint;
}