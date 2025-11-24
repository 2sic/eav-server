using System.Text.Json;
using System.Text.Json.Serialization;

namespace ToSic.Eav.Apps.Sys.FileSystemState;


/// <summary>
/// Manifest structure for extension.json files.
/// </summary>
/// <remarks>
/// This record represents the stable contract for extension.json files.
/// Properties are intentionally minimal and well-defined.
/// </remarks>
public sealed record ExtensionManifest
{
    /// <summary>
    /// The input type identifier (e.g., "string-font-icon").
    /// If empty/null, the extension is not an input type.
    /// </summary>
    [JsonPropertyName("inputTypeInside")]
    public string? InputTypeInside { get; init; }

    /// <summary>
    /// Asset paths for the input type UI.
    /// Can be a string, object with "default" key, or array.
    /// </summary>
    [JsonPropertyName("inputTypeAssets")]
    public JsonElement InputTypeAssets { get; init; }

    /// <summary>
    /// Indicates if this extension supports multiple editions (e.g., live, staging).
    /// When true, the loader will search for edition-specific versions.
    /// </summary>
    [JsonPropertyName("editionsSupported")]
    public bool EditionsSupported { get; init; }
}
