namespace ToSic.Eav.ImportExport.Json.Schema;

/// <summary>
/// See https://json-schema.org/draft/2020-12/draft-bhutton-json-schema-01
/// </summary>
public class JsonSchema
{
    /// <summary>
    /// The name, such as the field name or the content-type name.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Optional description.
    /// </summary>
    [JsonIgnore(Condition = WhenWritingDefault)]
    public string? Description { get; init; }

    /// <summary>
    /// The data type such as `array`, `boolean`, `string` etc.
    /// </summary>
    public string Type { get; init; }

    [JsonIgnore(Condition = WhenWritingDefault)]
    public string? Format { get; init; }

    /// <summary>
    /// List of possible values; can also be a reference to other schemas with that list?
    /// </summary>
    /// <remarks>
    /// https://www.learnjsonschema.com/2020-12/applicator/items/
    /// </remarks>
    [JsonIgnore(Condition = WhenWritingDefault)]
    public Dictionary<string, object>? Items { get; init; }

    /// <summary>
    /// Properties
    /// </summary>
    public Dictionary<string, JsonSchema>? Properties { get; init; }


    /// <summary>
    /// Array listing names of <see cref="Properties"/> which are required.
    /// </summary>
    [JsonIgnore(Condition = WhenWritingDefault)]
    public IEnumerable<string>? Required { get; init; }

    /// <summary>
    /// Determines if other properties are allowed or not.
    /// </summary>
    /// <remarks>
    /// Should probably default to false in most scenarios for 2sxc, but we didn't want to do this in this object.
    /// </remarks>
    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? AdditionalProperties { get; init; }
}