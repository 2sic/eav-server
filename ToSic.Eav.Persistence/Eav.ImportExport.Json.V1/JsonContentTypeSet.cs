

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json.V1;

public record JsonContentTypeSet
{
    ///// <summary>
    ///// V1 - header information
    ///// </summary>
    //[JsonPropertyOrder(-100)] // make sure it's always on top for clarity
    //public JsonHeader _ = new JsonHeader();

    /// <summary>
    /// V1 - a single Content-Type
    /// </summary>
    [JsonPropertyOrder(10)]
    [JsonIgnore(Condition = WhenWritingNull)] 
    public JsonContentType? ContentType { get; init; }


    /// <summary>
    /// V1.2 - A list of entities - added in 2sxc 12 to support content-types with additional sub-entities like formulas
    /// </summary>
    /// <remarks>
    /// Minor change in v20, now `ICollection{JsonEntity}` instead of `List{JsonEntity}` to support serialization of empty lists.
    /// </remarks>
    [JsonPropertyOrder(20)]
    [JsonIgnore(Condition = WhenWritingDefault)]
    public IList<JsonEntity>? Entities { get; init; }

}