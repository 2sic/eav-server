using ToSic.Eav.Apps.Sys.Paths;
using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys.ZoneMapper;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Apps.Sys.FileSystemState;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppFileSystemInputTypesLoader(ISite siteDraft, Generator<FileSystemLoader> fslGenerator, LazySvc<IAppPathsMicroSvc> appPathsLazy, LazySvc<IZoneMapper> zoneMapper)
    : AppFileSystemLoaderBase(siteDraft, appPathsLazy, zoneMapper, connect: [fslGenerator]), IAppInputTypesLoader
{
    /// <inheritdoc />
    public ICollection<InputTypeInfo> InputTypes()
    {
        var l = Log.Fn<ICollection<InputTypeInfo>>();
        // Collect from new canonical /extensions first (preferred), then legacy /system/ as fallback.
        var found = new List<InputTypeInfo>();

        // Local app paths
        MergeInputTypes(found, GetInputTypes(ExtensionsPath, AppConstants.AppPathPlaceholder, FolderConstants.AppExtensionsFolder));
        MergeInputTypes(found, GetInputTypes(ExtensionsLegacyPath, AppConstants.AppPathPlaceholder, FolderConstants.AppExtensionsLegacyFolder));

        // Shared app paths
        MergeInputTypes(found, GetInputTypes(ExtensionsPathShared, AppConstants.AppPathSharedPlaceholder, FolderConstants.AppExtensionsFolder));
        MergeInputTypes(found, GetInputTypes(ExtensionsLegacyPathShared, AppConstants.AppPathSharedPlaceholder, FolderConstants.AppExtensionsLegacyFolder));

        return l.Return(found, $"{found.Count}");
    }


    #region Helpers

    private ICollection<InputTypeInfo> GetInputTypes(string path, string placeholder, string folderName)
    {
        var l = Log.Fn<ICollection<InputTypeInfo>>();
        var di = new DirectoryInfo(path);
        if (!di.Exists)
            return l.Return([], "directory not found");
        var inputFolders = di.GetDirectories(FieldFolderPrefix + "*");
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
                // TODO: use metadata information if available
                return new InputTypeInfo
                {
                    Type = fullName,
                    Label = niceName,
                    Description = "Extension Field",
                    Assets = "",
                    AngularAssets = $"{placeholder}/{folderName}/{name}/{JsFile}",
                    DisableI18n = false,
                    UseAdam = false,
                    Source = "file-system",
                };
            })
            .ToListOpt();
        return l.Return(types, $"{types.Count}");
    }

    /// <summary>
    /// Merge input types into the accumulator, preferring already-present types (so earlier calls win).
    /// </summary>
    private static void MergeInputTypes(List<InputTypeInfo> acc, ICollection<InputTypeInfo> next)
    {
        if (next.Count == 0) return;
        var existing = acc.Select(t => t.Type).ToHashSet(StringComparer.OrdinalIgnoreCase);
        acc.AddRange(next.Where(t => !existing.Contains(t.Type)));
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