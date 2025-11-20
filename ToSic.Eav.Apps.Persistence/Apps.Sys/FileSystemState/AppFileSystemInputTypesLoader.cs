using ToSic.Eav.Apps.Sys.Paths;
using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys.ZoneMapper;
using ToSic.Eav.Persistence.File;

namespace ToSic.Eav.Apps.Sys.FileSystemState;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppFileSystemInputTypesLoader(ISite siteDraft, Generator<FileSystemLoader> fslGenerator, LazySvc<IAppPathsMicroSvc> appPathsLazy, LazySvc<IZoneMapper> zoneMapper)
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

        return l.Return(inputTypes, $"count:{inputTypes.Count}");

        // Merge input types into the accumulator, preferring already-present types (so earlier calls win).
        static ICollection<InputTypeInfo> MergeInputTypes(ICollection<InputTypeInfo> acc, ICollection<InputTypeInfo> next)
        {
            if (next.Count == 0) return acc;
            if (acc.Count == 0) return next;
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
        // Check if we have an /extensions/ folder - or the older /systems/ folder; exit early
        var di = new DirectoryInfo(path);
        if (!di.Exists)
            return l.Return([], "directory not found");

        // TODO: @STV
        // Goal is that we can have the extension.json tell us more about the fields, and possibly load additional editions.
        // This way we could have this setup:
        // - /extensions/something/ (v01.00)
        // - /staging/extensions/something/ (v01.01)
        //
        // So we want to change the logic below as follows:
        // 1. Look for all "root" /extension/* folders which either have an 'extension.json' or have the prefix as mentioned below
        // 2. If it has an extension.json, this is the leading information
        // 2.1. If this extension.json has a field `inputTypeInside` then use the value in the `inputTypeAssets` for the UiAssets
        // 2.2. Otherwise, skip
        // 3. If it does not have an extension.json, but has the folder-prefix as mentioned below, use the existing logic
        //
        // Now comes the new part
        // 4. If it has the extension.json, check if it has `editionsSupported` == true
        // 4.1. If yes, look for subfolders /live/extensions/[same-extension-name]/* and if found, load that `extension.json`
        // 4.2. ...to create the dictionary with UiAssets per edition
        //      So it would have 'default' = /extensions/something/index.js, and 'staging' = /staging/extensions/something/index.js
        //
        // Some notes
        // - make sure you use constants for `extension.json` etc. - currently in 2sxc, must be moved to EAV
        // - Use a prepared object to deserialize, which has these known properties, make sure you use a c# record to model it,
        // - don't navigate the json object manually, as these fields will be very "stable" and the code should represent that
        // - The UI doesn't know about these editions yet, so it won't have an effect yet. We'll do that once the backend works. 

        // Get folders beginning with "field-"
        var inputFolders = di.GetDirectories($"{FieldFolderPrefix}*");
        l.A($"found {inputFolders.Length} field-directories");

        var withIndexJs = inputFolders
            .Where(fld => fld.GetFiles(JsFile).Any())
            .Select(fld => fld.Name)
            .ToArray();
        l.A($"found {withIndexJs.Length} folders with {JsFile}");

        var types = withIndexJs
            .Select(name =>
            {
                var fullName = name.Substring(FieldFolderPrefix.Length);
                var niceName = InputTypeNiceName(name);
                var defaultAssets = $"{placeholder}/{folderName}/{name}/{JsFile}";
                // TODO: use metadata information if available
                return new InputTypeInfo
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
            })
            .ToListOpt();
        return l.Return(types, $"{types.Count}");
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


    #endregion
}