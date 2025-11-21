using System.Text.Json;
using System.Text.Json.Serialization;
using ToSic.Eav.Apps.Sys.Paths;
using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys.ZoneMapper;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Sys;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Sys.FileSystemState;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppFileSystemInputTypesLoader(ISite siteDraft,
    Generator<FileSystemLoader> fslGenerator,
    LazySvc<IAppPathsMicroSvc> appPathsLazy,
    LazySvc<IZoneMapper> zoneMapper)
    : AppFileSystemLoaderBase(siteDraft, appPathsLazy, zoneMapper, connect: [fslGenerator]), IAppInputTypesLoader
{
    /// <inheritdoc />
    public ICollection<InputTypeInfo> InputTypes()
    {
        var l = Log.Fn<ICollection<InputTypeInfo>>();

        // Local app paths
        var inputTypes = GetInputTypes(ExtensionsPath, AppConstants.AppPathPlaceholder, ExtensionsFolder);

        // Shared app paths, merge in, but don't override any existing ones
        inputTypes = MergeInputTypes(inputTypes, GetInputTypes(ExtensionsPathShared, AppConstants.AppPathSharedPlaceholder, ExtensionsFolderShared));

        return l.Return(inputTypes, $"OK, count:{inputTypes.Count}");

        // Merge input types into the accumulator, preferring already-present types (so earlier calls win).
        static ICollection<InputTypeInfo> MergeInputTypes(ICollection<InputTypeInfo> acc, ICollection<InputTypeInfo> next)
        {
            if (next.Count == 0)
                return acc;
            if (acc.Count == 0)
                return next;
            var existing = acc
                .Select(t => t.Type)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var uniqueNew = next
                .Where(t => !existing.Contains(t.Type))
                .ToListOpt();
            return acc.Concat(uniqueNew).ToListOpt();
        }
    }


    #region Helpers

    private ICollection<InputTypeInfo> GetInputTypes(string path, string placeholder, string folderName)
    {
        var l = Log.Fn<ICollection<InputTypeInfo>>();
        var di = new DirectoryInfo(path);
        if (!di.Exists)
            return l.Return([], "directory not found");

        var types = new List<InputTypeInfo>();
        foreach (var extensionFolder in di.GetDirectories())
        {
            var manifestFile = new FileInfo(Path.Combine(extensionFolder.FullName, FolderConstants.DataFolderProtected, FolderConstants.AppExtensionJsonFile));
            if (manifestFile.Exists)
            {
                var manifestType = InputTypeFromManifest(manifestFile, extensionFolder, placeholder, folderName);
                if (manifestType != null)
                {
                    types.Add(manifestType);
                    continue;
                }
            }

            // Fallback to legacy field-* folders with index.js
            if (!extensionFolder.Name.StartsWith(FieldFolderPrefix, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!extensionFolder.GetFiles(JsFile).Any())
                continue;

            types.Add(CreateLegacyInputType(extensionFolder.Name, placeholder, folderName));
        }

        return l.Return(types, $"OK, count:{types.Count}");
    }


    private static string InputTypeNiceName(string name)
    {
        var nameStack = name.Split('-');
        if (nameStack.Length < 3)
            return "[Bad Name Format]";
        // drop "field-" and "string-" or whatever type name is used
        nameStack = nameStack.Skip(2)
            .ToArray();
        var caps = nameStack
            .Select(n =>
            {
                if (string.IsNullOrWhiteSpace(n)) return "";
                if (n.Length <= 1) return n;
                return char.ToUpper(n[0]) + n.Substring(1);
            });

        var niceName = string.Join(" ", caps);
        return niceName;
    }

    private InputTypeInfo? InputTypeFromManifest(FileInfo manifestFile, DirectoryInfo extensionFolder, string placeholder, string folderName)
    {
        var l = Log.Fn<InputTypeInfo?>($"manifest:'{manifestFile.Name}', extension:'{extensionFolder.Name}', placeholder:'{placeholder}', folder:'{folderName}'");
        
        var manifest = LoadManifest(manifestFile);
        if (manifest?.InputTypeInside.IsEmpty() ?? true)
        {
            l.A("Manifest is null or InputTypeInside is empty");
            return l.ReturnNull("no valid manifest");
        }

        l.A($"Building UI assets for inputType:'{manifest.InputTypeInside}'");
        var assets = BuildUiAssets(manifest, extensionFolder, placeholder, folderName);
        
        var result = new InputTypeInfo
        {
            Type = manifest.InputTypeInside!,
            Label = InputTypeNiceName(extensionFolder.Name),
            Description = "Extension Field",
            UiAssets = assets,
            DisableI18n = false,
            UseAdam = false,
            Source = "file-system",
        };
        
        return l.Return(result, $"OK, type:'{result.Type}', assets count:{assets.Count}");
    }

    private InputTypeInfo CreateLegacyInputType(string folderName, string placeholder, string folderNameContainer)
    {
        var l = Log.Fn<InputTypeInfo>($"folder:'{folderName}', placeholder:'{placeholder}', container:'{folderNameContainer}'");
        
        var fullName = folderName.Substring(FieldFolderPrefix.Length);
        var niceName = InputTypeNiceName(folderName);
        var defaultAssets = $"{placeholder}/{folderNameContainer}/{folderName}/{JsFile}";
        
        l.A($"Legacy type: fullName='{fullName}', niceName='{niceName}', assets='{defaultAssets}'");
        
        var result = new InputTypeInfo
        {
            Type = fullName,
            Label = niceName,
            Description = "Extension Field",
            UiAssets = new Dictionary<string, string>
            {
                { InputTypeInfo.DefaultAssets, defaultAssets }
            },
            DisableI18n = false,
            UseAdam = false,
            Source = "file-system",
        };
        
        return l.Return(result, $"OK, type:'{fullName}'");
    }

    /// <summary>
    /// Build UI assets dictionary for an input type, including edition-specific assets if supported.
    /// </summary>
    /// <param name="manifest">The manifest of the primary extension</param>
    /// <param name="extensionFolder">The directory of the primary extension (e.g., /extensions/field-string-font-icon)</param>
    /// <param name="placeholder">Path placeholder token (e.g., [App:Path])</param>
    /// <param name="folderName">'extensions' folder</param>
    /// <returns>Dictionary mapping edition names to asset paths</returns>
    private Dictionary<string, string> BuildUiAssets(InputTypeManifest manifest, DirectoryInfo extensionFolder, string placeholder, string folderName)
    {
        var l = Log.Fn<Dictionary<string, string>>($"extension:{extensionFolder.Name}, editionsSupported:{manifest.EditionsSupported}");
        var assets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        // Always add the default asset
        var defaultAsset = AssetFromManifest(manifest, placeholder, extensionFolder.Name);
        assets[InputTypeInfo.DefaultAssets] = defaultAsset.HasValue()
            ? defaultAsset
            : $"{placeholder}/{folderName}/{extensionFolder.Name}/{JsFile}";

        // If editions are not supported, return with just the default asset
        if (!manifest.EditionsSupported)
            return l.Return(assets, $"editions not supported, count:{assets.Count}");

        // Navigate to app root: from /extensions/field-xyz -> /extensions -> /app-root
        var extensionsRoot = extensionFolder.Parent;
        var appRoot = extensionsRoot?.Parent;
        if (extensionsRoot == null || appRoot == null)
        {
            l.A($"Cannot navigate to app root from {extensionFolder.FullName}");
            return l.Return(assets, $"no app root found, count:{assets.Count}");
        }

        // Look for edition folders at the app root level (e.g., /staging, /live, /dev)
        var editionCount = 0;
        foreach (var editionFolder in appRoot.GetDirectories())
        {
            // Skip the current extensions root folder (don't process /extensions as an edition)
            if (editionFolder.Name.Equals(extensionsRoot.Name, StringComparison.OrdinalIgnoreCase))
                continue;

            // Check if this edition folder has a matching extensions subfolder
            // e.g., /staging/extensions/
            var editionExtensionsPath = Path.Combine(editionFolder.FullName, folderName);
            if (!Directory.Exists(editionExtensionsPath))
                continue;

            // Check if the specific extension exists in this edition
            // e.g., /staging/extensions/field-string-font-icon/
            var editionExtensionFolder = new DirectoryInfo(Path.Combine(editionExtensionsPath, extensionFolder.Name));
            if (!editionExtensionFolder.Exists)
                continue;

            // Check if there's a manifest file in the edition extension folder
            var editionManifestFile = new FileInfo(Path.Combine(editionExtensionFolder.FullName, FolderConstants.DataFolderProtected, FolderConstants.AppExtensionJsonFile));
            if (!editionManifestFile.Exists)
                continue;

            // Load and validate the edition manifest
            var editionManifest = LoadManifest(editionManifestFile);
            if (editionManifest?.InputTypeInside.IsEmpty() ?? true)
                continue;
            
            // Ensure the edition manifest references the same input type
            if (!editionManifest.InputTypeInside.Equals(manifest.InputTypeInside, StringComparison.OrdinalIgnoreCase))
            {
                l.A($"Edition {editionFolder.Name} has mismatched inputTypeInside: {editionManifest.InputTypeInside} != {manifest.InputTypeInside}");
                continue;
            }

            // Build the asset path for this edition
            var editionAsset = AssetFromManifest(editionManifest, placeholder, extensionFolder.Name, editionFolder.Name);
            assets[editionFolder.Name] = editionAsset.HasValue()
                ? editionAsset
                : $"{placeholder}/{editionFolder.Name}/{folderName}/{extensionFolder.Name}/{JsFile}";
            
            editionCount++;
        }

        return l.Return(assets, $"editions found:{editionCount}, total assets:{assets.Count}");
    }

    /// <summary>
    /// Extract and normalize asset path from manifest.
    /// </summary>
    /// <param name="manifest">The input type manifest</param>
    /// <param name="placeholder">Path placeholder token</param>
    /// <param name="extensionName">Extension folder name</param>
    /// <param name="editionName">Optional edition name (e.g., "staging")</param>
    /// <returns>Normalized asset path or null</returns>
    private string? AssetFromManifest(InputTypeManifest manifest, string placeholder, string extensionName, string? editionName = null)
    {
        var l = Log.Fn<string?>($"extension:'{extensionName}', edition:'{editionName}', placeholder:'{placeholder}'");
        
        var raw = manifest.InputTypeAssets.ValueKind switch
        {
            JsonValueKind.String => manifest.InputTypeAssets.GetString(),
            JsonValueKind.Object => manifest.InputTypeAssets.TryGetProperty(InputTypeInfo.DefaultAssets, out var def)
                ? def.GetString()
                : null,
            JsonValueKind.Array => manifest.InputTypeAssets.EnumerateArray().FirstOrDefault().GetString(),
            _ => null
        };

        if (raw.IsEmpty())
        {
            l.A("No asset found in manifest");
            return l.ReturnNull("raw asset empty");
        }

        var trimmed = raw.Trim();
        l.A($"Raw asset: '{raw}', trimmed: '{trimmed}'");
        
        // If it's already an absolute path or token, return as-is
        if (trimmed.StartsWith("[", StringComparison.OrdinalIgnoreCase) // token like [App:Path]
            || trimmed.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("/"))
        {
            l.A($"Asset is absolute/token, returning as-is");
            return l.Return(trimmed, "absolute path");
        }

        // Build relative path with optional edition prefix
        var basePath = placeholder.TrimEnd('/', '\\');
        var editionPrefix = editionName.HasValue() ? $"{editionName}/" : "";
        var normalized = trimmed.TrimStart('/');
        var result = $"{basePath}/{editionPrefix}{normalized}";
        
        return l.Return(result, $"normalized: base='{basePath}', edition='{editionPrefix}', file='{normalized}'");
    }

    private InputTypeManifest? LoadManifest(FileInfo manifestFile)
    {
        var l = Log.Fn<InputTypeManifest?>($"file:'{manifestFile.Name}'");
        try
        {
            var json = File.ReadAllText(manifestFile.FullName);
            if (json.IsEmpty())
            {
                l.A("JSON content is empty");
                return l.ReturnNull("empty json");
            }
            
            var result = JsonSerializer.Deserialize<InputTypeManifest>(json, ManifestSerializerOptions);
            return l.Return(result, $"inputType:'{result?.InputTypeInside}', editionsSupported:{result?.EditionsSupported}");
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            return l.ReturnNull("exception during load");
        }
    }

    private static readonly JsonSerializerOptions ManifestSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// Manifest structure for extension.json files defining input types.
    /// </summary>
    /// <remarks>
    /// This record represents the stable contract for extension.json files.
    /// Properties are intentionally minimal and well-defined.
    /// </remarks>
    private sealed record InputTypeManifest
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

    #endregion
}