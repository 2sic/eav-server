using System.Text.Json;
using ToSic.Eav.Apps.Sys.Extensions;
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
    LazySvc<IZoneMapper> zoneMapper,
    ExtensionManifestService manifestService)
    : AppFileSystemLoaderBase(siteDraft, appPathsLazy, zoneMapper, connect: [fslGenerator, manifestService]), IAppInputTypesLoader
{
    /// <inheritdoc />
    public ICollection<InputTypeInfo> InputTypes()
    {
        var l = Log.Fn<ICollection<InputTypeInfo>>();

        // Local app paths
        var inputTypes = GetInputTypes(ExtensionsPath, AppConstants.AppPathPlaceholder);

        // Shared app paths, merge in, but don't override any existing ones
        inputTypes = MergeInputTypes(inputTypes, GetInputTypes(ExtensionsPathShared, AppConstants.AppPathSharedPlaceholder));

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

    private ICollection<InputTypeInfo> GetInputTypes(string path, string placeholder)
    {
        var l = Log.Fn<ICollection<InputTypeInfo>>();
        var di = new DirectoryInfo(path);
        if (!di.Exists)
            return l.Return([], "directory not found");

        var types = new List<InputTypeInfo>();
        foreach (var extensionFolder in di.GetDirectories())
        {
            var manifestFile = ExtensionManifestService.GetManifestFileInfo(extensionFolder.FullName);
            if (manifestFile.Exists)
            {
                var manifestType = InputTypeFromManifest(manifestFile, extensionFolder, placeholder);
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

            types.Add(CreateLegacyInputType(extensionFolder.Name, placeholder));
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
                if (string.IsNullOrWhiteSpace(n))
                    return "";
                if (n.Length <= 1) return n;
                return char.ToUpper(n[0]) + n.Substring(1);
            });

        var niceName = string.Join(" ", caps);
        return niceName;
    }

    private InputTypeInfo? InputTypeFromManifest(FileInfo manifestFile, DirectoryInfo extensionFolder, string placeholder)
    {
        var l = Log.Fn<InputTypeInfo?>($"manifest:'{manifestFile.Name}', extension:'{extensionFolder.Name}', placeholder:'{placeholder}'");
        
        var manifest = manifestService.LoadManifest(manifestFile);
        if (manifest?.InputFieldInside ?? true)
        //if (manifest?.InputTypeInside.IsEmpty() ?? true)
        {
            l.A("Manifest is null or InputTypeInside is empty");
            return l.ReturnNull("no valid manifest");
        }

        //l.A($"Building UI assets for inputType:'{manifest.InputTypeInside}'");
        l.A($"Building UI assets for inputType:'{manifest.InputFieldInside}'");
        var assets = BuildUiAssets(manifest, extensionFolder, placeholder);
        
        var result = new InputTypeInfo
        {
            //Type = manifest.InputTypeInside!,
            Type = extensionFolder.Name,
            Label = InputTypeNiceName(extensionFolder.Name),
            Description = "Extension Field",
            UiAssets = assets,
            DisableI18n = false,
            UseAdam = false,
            Source = "file-system",
        };
        
        return l.Return(result, $"OK, type:'{result.Type}', assets count:{assets.Count}");
    }

    private InputTypeInfo CreateLegacyInputType(string folderName, string placeholder)
    {
        var l = Log.Fn<InputTypeInfo>($"folder:'{folderName}', placeholder:'{placeholder}'");
        
        var fullName = folderName.Substring(FieldFolderPrefix.Length);
        var niceName = InputTypeNiceName(folderName);
        var defaultAssets = $"{placeholder}/{FolderConstants.AppExtensionsFolder}/{folderName}/{JsFile}";
        
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
    /// <param name="extensionFolder">The directory of the primary extension (e.g., /Extensions/field-string-font-icon)</param>
    /// <param name="placeholder">Path placeholder token (e.g., [App:Path])</param>
    /// <returns>Dictionary mapping edition names to asset paths</returns>
    private Dictionary<string, string> BuildUiAssets(ExtensionManifest manifest, DirectoryInfo extensionFolder, string placeholder)
    {
        var l = Log.Fn<Dictionary<string, string>>($"extension:{extensionFolder.Name}, editionsSupported:{manifest.EditionsSupported}");
        var assets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        // Always add the default asset
        var defaultAsset = AssetFromManifest(manifest, placeholder, extensionFolder.Name);
        assets[InputTypeInfo.DefaultAssets] = defaultAsset.HasValue()
            ? defaultAsset
            : $"{placeholder}/{FolderConstants.AppExtensionsFolder}/{extensionFolder.Name}/{JsFile}";

        // If editions are not supported, return with just the default asset
        if (!manifest.EditionsSupported)
            return l.Return(assets, $"editions not supported, count:{assets.Count}");

        // Navigate to app root: from /Extensions/field-xyz -> /Extensions -> /app-root
        var extensionsRoot = extensionFolder.Parent;
        var appRoot = extensionsRoot?.Parent;
        if (extensionsRoot == null || appRoot == null)
        {
            l.A($"Cannot navigate to app root from {extensionFolder.FullName}");
            return l.Return(assets, $"no app root found, count:{assets.Count}");
        }

        // Figure out possible folders which could be sub-editions, check if the manifest exists, load, etc.
        var editionManifests = appRoot
            .GetDirectories()
            // Skip the current extensions root folder (don't process /Extensions as an edition)
            .Where(dirInfo => !dirInfo.Name.EqualsInsensitive(extensionsRoot.Name))
            // Get the FileInfo for the edition path, like /staging/Extensions/field-string-font-icon/extension.json
            .Select(dirInfo => new
            {
                Edition = dirInfo.Name,
                FileInfo = ExtensionManifestService.GetManifestFileInfo(Path.Combine(dirInfo.FullName, FolderConstants.AppExtensionsFolder, extensionFolder.Name))
            })
            // Verify it exists
            .Where(f => f.FileInfo.Exists)
            // Load the manifest
            .Select(f => new
            {
                f.Edition,
                Manifest = manifestService.LoadManifest(f.FileInfo)
            })
            // Verify not null
            .Where(f => f.Manifest != null)
            .ToListOpt();
        
        // Look for edition folders at the app root level (e.g., /staging, /live, /dev)
        var editionCount = 0;
        foreach (var editionAndManifest in editionManifests)
        {
            // Ensure the edition manifest references the same input type
            if (editionAndManifest.Manifest!.InputFieldInside != manifest.InputFieldInside)
            {
                l.A($"Edition {editionAndManifest.Edition} has mismatched inputFieldInside: {editionAndManifest.Manifest.InputFieldInside} != {manifest.InputFieldInside}");
                continue;
            }

            // Build the asset path for this edition
            var editionAsset = AssetFromManifest(editionAndManifest.Manifest, placeholder, extensionFolder.Name, editionAndManifest.Edition);
            assets[editionAndManifest.Edition] = editionAsset.HasValue()
                ? editionAsset
                : $"{placeholder}/{editionAndManifest.Edition}/{FolderConstants.AppExtensionsFolder}/{extensionFolder.Name}/{JsFile}";
            
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
    private string? AssetFromManifest(ExtensionManifest manifest, string placeholder, string extensionName, string? editionName = null)
    {
        var l = Log.Fn<string?>($"extension:'{extensionName}', edition:'{editionName}', placeholder:'{placeholder}'");

        // Note: it's not clear why this is so complicated
        // 2dm thinks (2026-08-20) that this is a left over when the `InputFieldAssets` could have had many types,
        // but this is probably obsolete; needs verification
        // TODO: @STV - pls check, and if it is not necessary anymore, simplify everything; otherwise add comments to clarify
        var raw = manifest.InputFieldAssets.ValueKind switch
        {
            JsonValueKind.String => manifest.InputFieldAssets.GetString(),
            JsonValueKind.Object => manifest.InputFieldAssets.TryGetProperty(InputTypeInfo.DefaultAssets, out var def)
                ? def.GetString()
                : null,
            JsonValueKind.Array => manifest.InputFieldAssets.EnumerateArray().FirstOrDefault().GetString(),
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

    #endregion
}