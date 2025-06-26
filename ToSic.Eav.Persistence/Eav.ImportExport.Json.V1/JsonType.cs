

// ReSharper disable once CheckNamespace

using ToSic.Eav.Data.Sys.ContentTypes;

namespace ToSic.Eav.ImportExport.Json.V1;

public class JsonType
{
    /// <summary>
    /// Main static identifier, usually a guid, in rare cases a string such as "@string-dropdown"
    /// </summary>
    /// <remarks>
    /// Should be NameId, but was created a long time ago, and a rename would cause too much trouble.
    /// </remarks>
    [JsonPropertyOrder(1)]
    public string Id { get; init; } = null!;    // is always set by constructors or de-serialization

    /// <summary>
    /// Nice name, string
    /// </summary>
    [JsonPropertyOrder(2)]
    public string Name { get; init; } = null!;  // is always set by constructors or de-serialization

    /// <summary>
    /// Map for attribute name to long-term-guid (if given)
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    [JsonPropertyOrder(100)]
    public IDictionary<string, Guid>? AttributeMap { get; set; }

    // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
    /// <summary>
    /// Additional description for the type, usually not included.
    /// Sometimes added in admin-UI scenarios, where additional info is useful
    /// ATM only used for Metadata
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    public string? Description { get; set; }

    /// <summary>
    /// Additional description for the type, usually not included.
    /// Sometimes added in admin-UI scenarios, where additional info is useful
    /// ATM only used for Metadata
    /// </summary>
    /// <remarks>Added in v13.02</remarks>
    [JsonIgnore(Condition = WhenWritingNull)]
    public string? Title { get; set; }

    /// <summary>
    /// Empty constructor is important for de-serializing
    /// </summary>
    public JsonType() { }

    /// <summary>
    /// Constructor to create a JsonType from an IEntity, with options to include description and attribute map.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="withDescription"></param>
    /// <param name="withMap"></param>
    public JsonType(IEntity entity, bool withDescription = false, bool withMap = false)
        : this(entity.Type, withDescription, withMap) { }

    public JsonType(IContentType type, bool withDescription = false, bool withMap = false)
    {
        Name = type.Name;
        Id = type.NameId;
        if (withDescription)
        {
            var description = type.DetailsOrNull();
            Title = description?.Title ?? type.NameId;
            Description = description?.Description ?? "";
        }

        if (withMap)
        {
            var withGuid = type.Attributes
                .Where(a => a.Guid != null && a.Guid != Guid.Empty)
                .ToDictionary(a => a.Name, a => a.Guid ?? Guid.Empty);
            if (withGuid.Any())
                AttributeMap = withGuid;
        }
    }
}