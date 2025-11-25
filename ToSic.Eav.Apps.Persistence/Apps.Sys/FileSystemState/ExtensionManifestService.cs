using System.Text.Json;
using ToSic.Eav.Sys;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Sys.FileSystemState;

/// <summary>
/// Service for loading and working with extension.json manifests.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ExtensionManifestService() : ServiceBase("Ext.ManSvc")
{
    private static readonly JsonSerializerOptions ManifestSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// Load and deserialize an extension manifest from a file.
    /// </summary>
    /// <param name="manifestFile">The manifest file to load</param>
    /// <returns>Deserialized manifest or null if loading fails</returns>
    public ExtensionManifest? LoadManifest(FileInfo manifestFile)
    {
        var l = Log.Fn<ExtensionManifest?>($"file:'{manifestFile.Name}'");
        try
        {
            var json = File.ReadAllText(manifestFile.FullName);
            if (json.IsEmpty())
            {
                l.A("JSON content is empty");
                return l.ReturnNull("empty json");
            }
            
            var tempResult = JsonSerializer.Deserialize<ExtensionManifest>(json, ManifestSerializerOptions);
            if (tempResult == null)
                return l.ReturnNull("deserialize returned null");

            // Clone JsonElement properties to prevent "Operation is not valid due to the current state" errors
            // when the manifest is later serialized (JsonElements reference a backing JsonDocument that may be disposed)
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
            l.Ex(ex);
            return l.ReturnNull("exception during load");
        }
    }

    /// <summary>
    /// Clone a JsonElement to create a self-contained copy that survives JsonDocument disposal.
    /// </summary>
    private static JsonElement CloneJsonElement(JsonElement element)
    {
        // If the element is undefined/null, return it as-is
        if (element.ValueKind == JsonValueKind.Undefined || element.ValueKind == JsonValueKind.Null)
            return element;

        // Clone by round-tripping through serialization to create a new backing document
        using var doc = JsonDocument.Parse(element.GetRawText());
        return doc.RootElement.Clone();
    }

    /// <summary>
    /// Get the path to the manifest file for an extension folder.
    /// </summary>
    /// <param name="extensionFolder">The extension folder</param>
    /// <returns>FileInfo for the manifest file (may not exist)</returns>
    public FileInfo GetManifestFile(DirectoryInfo extensionFolder)
        => new(Path.Combine(extensionFolder.FullName, FolderConstants.DataFolderProtected, FolderConstants.AppExtensionJsonFile));

    #region Helper Methods

    public IReadOnlyList<Guid> GetReleaseGuids(ExtensionManifest manifest)
    {
        var list = new List<Guid>();
        var releases = manifest.Releases;
        if (releases.ValueKind == JsonValueKind.Array)
            foreach (var item in releases.EnumerateArray())
                if (item.ValueKind == JsonValueKind.String && Guid.TryParse(item.GetString(), out var g)) list.Add(g);
        return list;
    }

    public IReadOnlyList<Guid> GetBundleGuids(ExtensionManifest manifest)
    {
        var list = new List<Guid>();
        var bundles = manifest.Bundles;
        switch (bundles.ValueKind)
        {
            case JsonValueKind.String:
                var str = bundles.GetString();
                if (!string.IsNullOrWhiteSpace(str))
                    foreach (var part in str.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                        if (Guid.TryParse(part.Trim(), out var g)) list.Add(g);
                break;
            case JsonValueKind.Array:
                foreach (var item in bundles.EnumerateArray())
                    if (item.ValueKind == JsonValueKind.String && Guid.TryParse(item.GetString(), out var g)) list.Add(g);
                break;
        }
        return list;
    }

    public IReadOnlyList<string> GetInputTypeAssetPaths(ExtensionManifest manifest)
    {
        var list = new List<string>();
        var assets = manifest.InputTypeAssets;
        switch (assets.ValueKind)
        {
            case JsonValueKind.String:
                var s = assets.GetString();
                if (!string.IsNullOrWhiteSpace(s)) list.Add(s);
                break;
            case JsonValueKind.Object:
                if (assets.TryGetProperty("default", out var def) && def.ValueKind == JsonValueKind.String)
                {
                    var defStr = def.GetString();
                    if (!string.IsNullOrWhiteSpace(defStr)) list.Add(defStr);
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in assets.EnumerateArray())
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        var v = item.GetString();
                        if (!string.IsNullOrWhiteSpace(v)) list.Add(v);
                    }
                break;
        }
        return list;
    }

    #endregion
}
