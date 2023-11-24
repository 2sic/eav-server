﻿namespace ToSic.Eav.Serialization;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class MetadataForSerialization
{
    /// <summary>
    /// Should be serialized
    /// </summary>
    public bool? Serialize { get; set; }

    /// <summary>
    /// The key should be serialized
    /// </summary>
    public bool? SerializeKey { get; set; }

    /// <summary>
    /// The type should be serialized
    /// </summary>
    /// <remarks>
    /// As of 2021-11 I believe this setting is passed around, but never used
    /// </remarks>
    public bool? SerializeType { get; set; }

    public static MetadataForSerialization Stabilize(
        MetadataForSerialization original,
        MetadataForSerialization addition = null,
        bool serialize = false, bool key = false, bool type = false) =>
        new()
        {
            Serialize = original?.Serialize ?? addition?.Serialize ?? serialize,
            SerializeKey = original?.SerializeKey ?? addition?.SerializeKey ?? key,
            SerializeType = original?.SerializeType ?? addition?.SerializeType ?? type,
        };

}