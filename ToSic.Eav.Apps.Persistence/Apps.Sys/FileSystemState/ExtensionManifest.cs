using System.Text.Json;
using System.Text.Json.Serialization;

namespace ToSic.Eav.Apps.Sys.FileSystemState;

/// <summary>
/// Manifest structure for extension.json files.
/// </summary>
/// <remarks>
/// Stable contract for extension.json files. Pure data – helper logic lives in ExtensionManifestService.
/// </remarks>
public sealed record ExtensionManifest
{
    private static readonly JsonElement JsonNullElement = JsonDocument.Parse("null").RootElement.Clone();

    #region Identity & Basic Metadata

    /// <summary>Unique guid of the extension.</summary>
    [JsonPropertyName("guid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Guid { get; init; }

    /// <summary>Secondary name identifier (guid-like id).</summary>
    [JsonPropertyName("nameId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NameId { get; init; }

    /// <summary>Human readable name.</summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }

    /// <summary>Short teaser text.</summary>
    [JsonPropertyName("teaser")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Teaser { get; init; }

    /// <summary>Longer description text (can contain HTML).</summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    /// <summary>Author / creator.</summary>
    [JsonPropertyName("createdBy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CreatedBy { get; init; }

    /// <summary>Copyright / license info.</summary>
    [JsonPropertyName("copyright")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Copyright { get; init; }

    /// <summary>Main documentation / landing link.</summary>
    [JsonPropertyName("linkMain")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LinkMain { get; init; }

    /// <summary>Documentation link.</summary>
    [JsonPropertyName("linkDocs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LinkDocs { get; init; }

    /// <summary>Source code link.</summary>
    [JsonPropertyName("linkSource")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LinkSource { get; init; }

    /// <summary>Demo / showcase link.</summary>
    [JsonPropertyName("linkDemo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LinkDemo { get; init; }
    
    #endregion

    #region Release Information

    /// <summary>Extension version string (e.g. "1.0.0").</summary>
    [JsonPropertyName("version")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Version { get; init; }

    /// <summary>Releases entry. Flexible shape.</summary>
    [JsonPropertyName("releases")]
    public JsonElement Releases { get; init; } = JsonNullElement;

    /// <summary>Set by runtime to indicate the extension is installed.</summary>
    [JsonPropertyName("isInstalled")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsInstalled { get; init; }

    #endregion

    #region Platform Support

    /// <summary>Indicates if this extension supports multiple editions (e.g., live, staging).</summary>
    [JsonPropertyName("editionsSupported")]
    public bool EditionsSupported { get; init; }

    /// <summary>Supports generic 2sxc platform.</summary>
    [JsonPropertyName("sxcSupported")]
    public bool SxcSupported { get; init; }

    /// <summary>Minimum 2sxc version.</summary>
    [JsonPropertyName("sxcVersionMin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SxcVersionMin { get; init; }

    /// <summary>Supports DNN platform.</summary>
    [JsonPropertyName("dnnSupported")]
    public bool DnnSupported { get; init; }

    /// <summary>Minimum DNN version.</summary>
    [JsonPropertyName("dnnVersionMin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DnnVersionMin { get; init; }

    /// <summary>Supports Oqtane platform.</summary>
    [JsonPropertyName("oqtSupported")]
    public bool OqtSupported { get; init; }

    /// <summary>Minimum Oqtane version.</summary>
    [JsonPropertyName("oqtVersionMin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OqtVersionMin { get; init; }

    #endregion

    #region Capability Flags

    /// <summary>Indicates presence of field definitions.</summary>
    [JsonPropertyName("inputFieldInside")]
    public bool InputFieldInside { get; init; }

    /// <summary>Indicates presence of AppCode files.</summary>
    [JsonPropertyName("appCodeInside")]
    public bool AppCodeInside { get; init; }

    /// <summary>Indicates presence of WebApi endpoints.</summary>
    [JsonPropertyName("webApiInside")]
    public bool WebApiInside { get; init; }

    /// <summary>Indicates presence of Razor files.</summary>
    [JsonPropertyName("razorInside")]
    public bool RazorInside { get; init; }

    /// <summary>Indicates presence of data bundles (simple flag).</summary>
    [JsonPropertyName("hasDataBundles")]
    public bool HasDataBundles { get; init; }

    /// <summary>Indicates presence of content types.</summary>
    [JsonPropertyName("contentTypesInside")]
    public bool ContentTypesInside { get; init; }

    /// <summary>Indicates presence of queries.</summary>
    [JsonPropertyName("queriesInside")]
    public bool QueriesInside { get; init; }

    /// <summary>Indicates presence of views.</summary>
    [JsonPropertyName("viewsInside")]
    public bool ViewsInside { get; init; }

    /// <summary>Indicates whether the extension contains data that should be exported/imported.</summary>
    [JsonPropertyName("dataInside")]
    public bool DataInside { get; init; }

    [JsonPropertyName("resourcesContentType")]
    public string ResourcesContentType { get; init; }

    [JsonPropertyName("settingsContentType")]
    public string SettingsContentType { get; init; }

    #endregion

    #region Assets

    // TODO: @STV - this is always a string - why is it JsonElement here? and why are the docs saying it can be other things?
    /// <summary>
    /// Asset paths for the input field UI. Can be a string, object with "default" key, or array.
    /// </summary>
    [JsonPropertyName("inputFieldAssets")]
    public JsonElement InputFieldAssets { get; init; } = JsonNullElement;

    // 2025-12-02 2dm - this looks wrong and the field doesn't exist any more in extension.json files
    ///// <summary>The input type identifier (e.g., "string-font-icon"). If empty/null, the extension is not an input type.</summary>
    //[JsonPropertyName("inputTypeInside")]
    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    //public string? InputTypeInside { get; init; }

    // 2025-12-02 2dm - this looks wrong and the field doesn't exist any more in extension.json files
    ///// <summary>Asset paths for the input type UI. Can be a string, object with "default" key, or array.</summary>
    //[JsonPropertyName("inputTypeAssets")]
    //public JsonElement InputTypeAssets { get; init; } = JsonNullElement;

    // TODO: @STV - this is always a string - why is it JsonElement here? and why are the docs saying it can be other things?
    /// <summary>Data bundle references (expanded or raw). Flexible shape.</summary>
    [JsonPropertyName("dataBundles")]
    public JsonElement DataBundles { get; init; } = JsonNullElement;

    ///// <summary>Alternative bundles reference property (raw list / legacy usage).</summary>
    //[JsonPropertyName("bundles")]
    //public JsonElement Bundles { get; init; } = JsonNullElement;

    #endregion
}
