namespace ToSic.Eav.Serialization;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class SubEntitySerialization: ISubEntitySerialization
{
    public bool? Serialize { get; init; }

    public bool? SerializesAsCsv { get; init; }
        
    public bool? SerializeId { get; init; }

    public bool? SerializeGuid { get; init; }

    public bool? SerializeTitle { get; init; }

    public static ISubEntitySerialization AllTrue()
        => Stabilize(null, false, true, true, true, true);

    public static ISubEntitySerialization Stabilize(ISubEntitySerialization original,
        bool serialize = false, bool asCsv = false, bool id = false, bool guid = false, bool title = false) =>
        new SubEntitySerialization
        {
            Serialize = original?.Serialize ?? serialize,
            SerializesAsCsv = original?.SerializesAsCsv ?? asCsv,
            SerializeId = original?.SerializeId ?? id,
            SerializeGuid = original?.SerializeGuid ?? guid,
            SerializeTitle = original?.SerializeTitle ?? title
        };
    public static ISubEntitySerialization Stabilize(
        ISubEntitySerialization original, 
        ISubEntitySerialization addition = null, 
        bool serialize = false, bool id = false, bool guid = false, bool title = false) =>
        new SubEntitySerialization
        {
            Serialize = original?.Serialize ?? addition?.Serialize ?? serialize,
            SerializeId = original?.SerializeId ?? addition?.SerializeId ?? id,
            SerializeGuid = original?.SerializeGuid ?? addition?.SerializeGuid ?? guid,
            SerializeTitle = original?.SerializeTitle ?? addition?.SerializeTitle ?? title
        };
}