

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json.V1;

public class JsonContentTypeSet
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
    public JsonContentType ContentType;


    /// <summary>
    /// V1.2 - A list of entities - added in 2sxc 12 to support content-types with additional sub-entities like formulas
    /// </summary>
    [JsonPropertyOrder(20)]
    [JsonIgnore(Condition = WhenWritingDefault)]
    public IEnumerable<JsonEntity> Entities;

}