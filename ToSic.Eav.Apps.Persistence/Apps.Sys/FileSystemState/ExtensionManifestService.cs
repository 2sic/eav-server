using System.Text.Json;
using ToSic.Eav.Sys;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Sys.FileSystemState;

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
                InputTypeAssets = CloneJsonElement(tempResult.InputTypeAssets),
                DataBundles = CloneJsonElement(tempResult.DataBundles),
                Bundles = CloneJsonElement(tempResult.Bundles),
                Releases = CloneJsonElement(tempResult.Releases),
                InputFieldAssets = CloneJsonElement(tempResult.InputFieldAssets)
            };
            
            return l.Return(result, $"inputType:'{result.InputTypeInside}', version:'{result.Version}', editionsSupported:{result.EditionsSupported}");
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
        if (element.ValueKind == JsonValueKind.Undefined || element.ValueKind == JsonValueKind.Null)
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
    public FileInfo GetManifestFile(DirectoryInfo extensionFolder)
        => new(Path.Combine(extensionFolder.FullName, FolderConstants.DataFolderProtected, FolderConstants.AppExtensionJsonFile));

    #region Helper Methods

    /// <summary>
    /// Extract GUIDs from <see cref="ExtensionManifest.Releases"/> when it is an array of GUID strings.
    /// Returns an empty list if shape is different.
    /// </summary>
    public IReadOnlyList<Guid> GetReleaseGuids(ExtensionManifest manifest)
    {
        var list = new List<Guid>();
        var releases = manifest.Releases;
        if (releases.ValueKind == JsonValueKind.Array)
            foreach (var item in releases.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String)
                    continue;
                var s = item.GetString();
                if (s != null && Guid.TryParse(s, out var g))
                    list.Add(g);
            }
        return list;
    }

    /// <summary>
    /// Extract bundle GUIDs from <see cref="ExtensionManifest.Bundles"/>.
    /// Accepts single string (CSV supported) or array of strings.
    /// </summary>
    public IReadOnlyList<Guid> GetBundleGuids(ExtensionManifest manifest)
    {
        var list = new List<Guid>();
        var bundles = manifest.Bundles;
        switch (bundles.ValueKind)
        {
            case JsonValueKind.String:
                {
                    var str = bundles.GetString();
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        foreach (var part in str!.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                            if (Guid.TryParse(part.Trim(), out var g))
                                list.Add(g);
                    }
                    break;
                }
            case JsonValueKind.Array:
                foreach (var item in bundles.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.String)
                        continue;
                    var s = item.GetString();
                    if (s != null && Guid.TryParse(s, out var g))
                        list.Add(g);
                }
                break;
        }
        return list;
    }

    /// <summary>
    /// Normalize <see cref="ExtensionManifest.InputTypeAssets"/> to a list of relative asset paths.
    /// Supports string, object with a "default" string property, or array of strings.
    /// </summary>
    public IReadOnlyList<string> GetInputTypeAssetPaths(ExtensionManifest manifest)
    {
        var list = new List<string>();
        var assets = manifest.InputTypeAssets;
        switch (assets.ValueKind)
        {
            case JsonValueKind.String:
                {
                    var s = assets.GetString();
                    if (!string.IsNullOrWhiteSpace(s))
                        list.Add(s!);
                    break;
                }
            case JsonValueKind.Object:
                if (assets.TryGetProperty("default", out var def) && def.ValueKind == JsonValueKind.String)
                {
                    var defStr = def.GetString();
                    if (!string.IsNullOrWhiteSpace(defStr))
                        list.Add(defStr!);
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in assets.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.String)
                        continue;
                    var v = item.GetString();
                    if (!string.IsNullOrWhiteSpace(v))
                        list.Add(v!);
                }
                break;
        }
        return list;
    }

    #endregion
}
