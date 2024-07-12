namespace ToSic.Eav.Serialization;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ISubEntitySerialization : IEntityIdSerialization
{
    /// <summary>
    /// Should sub entities get serialized?
    /// </summary>
    bool? Serialize { get; }

    /// <summary>
    /// Format to serialize the sub entities in.
    /// * null, empty or "object" - object
    /// * "csv" - comma separated values as string
    /// * "array" json array
    /// </summary>
    /// <remarks>
    /// * Created v15.03 and was originally a bool? called SerializesAsCsv
    /// * Modified in v18 to be a string, to allow more formats
    /// </remarks>
    public string SerializeFormat { get; init; }
}