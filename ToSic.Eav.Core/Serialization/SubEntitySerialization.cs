namespace ToSic.Eav.Serialization;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class SubEntitySerialization: ISubEntitySerialization
{
    public bool? Serialize { get; init; }

    /// <summary>
    /// Expected values "csv", "array", "object" (default)
    /// </summary>
    public string SerializeFormat { get; init; }

    public bool? SerializeId { get; init; }

    public bool? SerializeGuid { get; init; }

    public bool? SerializeTitle { get; init; }

    public static ISubEntitySerialization NeverSerializeChildren()
        => Stabilize(null, false, "object", true, true, true);

    public static ISubEntitySerialization Stabilize(
        ISubEntitySerialization original,
        bool serialize,
        string format,
        bool id,
        bool guid,
        bool title)
        => new SubEntitySerialization
        {
            Serialize = original?.Serialize ?? serialize,
            SerializeFormat = original?.SerializeFormat ?? format ?? "object",
            SerializeId = original?.SerializeId ?? id,
            SerializeGuid = original?.SerializeGuid ?? guid,
            SerializeTitle = original?.SerializeTitle ?? title
        };

    public static ISubEntitySerialization Stabilize(
        ISubEntitySerialization original,
        ISubEntitySerialization addition = null,
        bool serialize = false,
        bool id = false,
        bool guid = false,
        bool title = false)
        => new SubEntitySerialization
        {
            Serialize = original?.Serialize ?? addition?.Serialize ?? serialize,
            SerializeId = original?.SerializeId ?? addition?.SerializeId ?? id,
            SerializeGuid = original?.SerializeGuid ?? addition?.SerializeGuid ?? guid,
            SerializeTitle = original?.SerializeTitle ?? addition?.SerializeTitle ?? title
        };
}