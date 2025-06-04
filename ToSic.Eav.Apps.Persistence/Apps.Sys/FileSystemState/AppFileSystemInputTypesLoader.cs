using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Context;
using ToSic.Eav.Integration;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Apps.Integration;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppFileSystemInputTypesLoader(ISite siteDraft, Generator<FileSystemLoader> fslGenerator, LazySvc<IAppPathsMicroSvc> appPathsLazy, LazySvc<IZoneMapper> zoneMapper)
    : AppFileSystemLoaderBase(siteDraft, appPathsLazy, zoneMapper, connect: [fslGenerator]), IAppInputTypesLoader
{
    public new IAppInputTypesLoader Init(IAppReader reader, LogSettings logSettings)
        => base.Init(reader, logSettings) as IAppInputTypesLoader;

    /// <inheritdoc />
    public List<InputTypeInfo> InputTypes()
    {
        var l = Log.Fn<List<InputTypeInfo>>();
        var types = GetInputTypes(Path, AppConstants.AppPathPlaceholder);
        types.AddRange(GetInputTypes(PathShared, AppConstants.AppPathSharedPlaceholder));
        return l.Return(types, $"{types.Count}");
    }



    #region Helpers

    private List<InputTypeInfo> GetInputTypes(string path, string placeholder)
    {
        var l = Log.Fn<List<InputTypeInfo>>();
        var di = new DirectoryInfo(path);
        if (!di.Exists) return l.Return([], "directory not found");
        var inputFolders = di.GetDirectories(FieldFolderPrefix + "*");
        Log.A($"found {inputFolders.Length} field-directories");

        var withIndexJs = inputFolders
            .Where(fld => fld.GetFiles(JsFile).Any())
            .Select(fld => fld.Name).ToArray();
        Log.A($"found {withIndexJs.Length} folders with {JsFile}");

        var types = withIndexJs.Select(name =>
            {
                var fullName = name.Substring(FieldFolderPrefix.Length);
                var niceName = InputTypeNiceName(name);
                // TODO: use metadata information if available
                return new InputTypeInfo(fullName, niceName, "Extension Field", "", false,
                    $"{placeholder}/{FolderConstants.FolderAppExtensions}/{name}/{JsFile}", false, "file-system");
            })
            .ToList();
        return l.Return(types, $"{types.Count}");
    }

    private static string InputTypeNiceName(string name)
    {
        var nameStack = name.Split('-');
        if (nameStack.Length < 3)
            return "[Bad Name Format]";
        // drop "field-" and "string-" or whatever type name is used
        nameStack = nameStack.Skip(2).ToArray();
        var caps = nameStack.Select(n =>
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