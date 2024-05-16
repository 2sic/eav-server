

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json.V1;

public class JsonFormat: JsonContentTypeSet
{
    /// <summary>
    /// V1 - header information
    /// </summary>
    [JsonPropertyOrder(-1000)] // make sure it's always on top for clarity
    public JsonHeader _ = new();
        
    /// <summary>
    /// Bundles in this package.
    /// Added ca. v15
    /// </summary>
    [JsonIgnore(Condition = WhenWritingDefault)]
    [JsonPropertyOrder(10)]
    public List<JsonBundle> Bundles { get; set; }

    /// <summary>
    /// V1 - a single Entity
    /// </summary>
    [JsonPropertyOrder(20)]
    [JsonIgnore(Condition = WhenWritingNull)] 
    public JsonEntity Entity;

    ///// <summary>
    ///// V1 - a single Content-Type
    ///// </summary>
    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    //public JsonContentType ContentType;


    ///// <summary>
    ///// V1.2 - A list of entities - added in 2sxc 12 to support content-types with additional sub-entities like formulas
    ///// </summary>
    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    //public IEnumerable<JsonEntity> Entities;


}