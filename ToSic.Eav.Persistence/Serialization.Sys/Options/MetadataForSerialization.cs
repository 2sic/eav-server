namespace ToSic.Eav.Serialization;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class MetadataForSerialization
{
    /// <summary>
    /// Should be serialized
    /// </summary>
    public bool? Serialize { get; init; }

    /// <summary>
    /// The key should be serialized
    /// </summary>
    public bool? SerializeKey { get; init; }

    /// <summary>
    /// The type should be serialized
    /// </summary>
    /// <remarks>
    /// As of 2021-11 I believe this setting is passed around, but never used
    /// </remarks>
    public bool? SerializeType { get; init; }

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