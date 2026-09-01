using System.Text.Json;
using ToSic.Eav.Sys;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Sys.Extensions;

/// <summary>
/// Service for loading and working with extension.json manifests.
/// </summary>
/// <remarks>
/// This service centralizes manifest loading and safe materialization of flexible JSON shapes.
///
/// Important: the <see cref="ExtensionManifest"/> contains a few <see cref="JsonElement"/> properties (for flexible shapes).
/// These elements reference a backing <see cref="JsonDocument"/> which may be disposed by the serializer.
/// To avoid "Operation is not valid due to the current state of the object" when later serializing the DTOs,
/// we clone these elements immediately after deserialization so they have an independent lifetime.
/// </remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ExtensionManifestService() : ServiceBase("Ext.ManSvc")
{
    /// <summary>
    /// Serializer options for reading <c>extension.json</c> files.
    /// - Case-insensitive property names
    /// - Trailing commas allowed
    /// - Comments skipped
    /// </summary>
    private static readonly JsonSerializerOptions ManifestSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// Load and deserialize an extension manifest from a file and clone volatile JsonElement members.
    /// </summary>
    /// <param name="manifestFile">The manifest file to load.</param>
    /// <returns>The deserialized <see cref="ExtensionManifest"/> or <c>null</c> if loading/parsing fails.</returns>
    public ExtensionManifest? LoadManifest(FileInfo manifestFile)
    {
        var l = Log.Fn<ExtensionManifest?>($"file:'{manifestFile.Name}'");
        try
        {
            // Read JSON content
            var json = File.ReadAllText(manifestFile.FullName);
            if (json.IsEmpty())
            {
                l.A("JSON content is empty");
                return l.ReturnNull("empty json");
            }
            
            // Deserialize to strong type (keeps flexible parts as JsonElement)
            var tempResult = JsonSerializer.Deserialize<ExtensionManifest>(json, ManifestSerializerOptions);
            if (tempResult == null)
                return l.ReturnNull("deserialize returned null");

            // Clone JsonElement properties to prevent invalid-state exceptions later when serializing the response.
            // JsonElement holds a pointer to a JsonDocument which may be disposed after this method returns.
            var result = tempResult with
            {
                DataBundles = CloneJsonElement(tempResult.DataBundles),
                Releases = CloneJsonElement(tempResult.Releases),
                InputFieldAssets = CloneJsonElement(tempResult.InputFieldAssets)
            };
            
            return l.Return(result, $"inputType:'{result.InputFieldInside}', version:'{result.Version}', editionsSupported:{result.EditionsSupported}");
        }
        catch (Exception ex)
        {
            // Log and surface null as a non-fatal failure for callers which may continue gracefully
            l.Ex(ex);
            return l.ReturnNull("exception during load");
        }
    }

    /// <summary>
    /// Clone a <see cref="JsonElement"/> to create a self-contained copy that survives <see cref="JsonDocument"/> disposal.
    /// </summary>
    /// <param name="element">The element to clone.</param>
    /// <returns>
    /// A cloned element (independent lifetime) or the original element when it is <see cref="JsonValueKind.Undefined"/> / <see cref="JsonValueKind.Null"/>.
    /// </returns>
    private static JsonElement CloneJsonElement(JsonElement element)
    {
        // If the element is undefined/null, return it as-is to keep semantics
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            return element;

        // Clone by parsing its raw text to a new document and cloning the root element
        using var doc = JsonDocument.Parse(element.GetRawText());
        return doc.RootElement.Clone();
    }

    /// <summary>
    /// Get the path to the manifest file for an extension folder.
    /// </summary>
    /// <param name="extensionFolder">The extension folder.</param>
    /// <returns>FileInfo for the manifest file (may not exist).</returns>
    public static FileInfo GetManifestFileInfo(string extensionFolder)
        => new(Path.Combine(extensionFolder, FolderConstants.DataFolderProtected, FolderConstants.AppExtensionJsonFile));

}
