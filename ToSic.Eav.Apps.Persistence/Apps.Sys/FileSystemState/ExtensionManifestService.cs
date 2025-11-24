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
            
            var result = JsonSerializer.Deserialize<ExtensionManifest>(json, ManifestSerializerOptions);
            return l.Return(result, $"inputType:'{result?.InputTypeInside}', editionsSupported:{result?.EditionsSupported}");
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            return l.ReturnNull("exception during load");
        }
    }

    /// <summary>
    /// Get the path to the manifest file for an extension folder.
    /// </summary>
    /// <param name="extensionFolder">The extension folder</param>
    /// <returns>FileInfo for the manifest file (may not exist)</returns>
    public FileInfo GetManifestFile(DirectoryInfo extensionFolder)
        => new(Path.Combine(extensionFolder.FullName, FolderConstants.DataFolderProtected, FolderConstants.AppExtensionJsonFile));
}
