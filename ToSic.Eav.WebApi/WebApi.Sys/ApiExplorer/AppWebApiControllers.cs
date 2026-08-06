using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.Sys;
using ToSic.Eav.WebApi.Sys.Admin;
using ToSic.Eav.WebApi.Sys.Dto;
using ToSic.Sys.Users;

namespace ToSic.Eav.WebApi.Sys.ApiExplorer;

[PrivateApi]
[VisualQuery(
    NiceName = "Web Api Controllers",
    NameId = "98e35962-ae3c-44b3-a3fd-1275419825c7",
    NameIds = ["System.AppWebApiControllers"],
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.Internal,
    UiHint = "Lists WebAPI controller files of an app"
)]
public class AppWebApiControllers : CustomDataSource
{
    private readonly IUser _user;
    private readonly LazySvc<IAppExplorerControllerDependency> _appFileController;

    public const string LogSuffix = "AppWebApiControllers";


    public AppWebApiControllers(
        Dependencies services,
        IUser user,
        LazySvc<IAppExplorerControllerDependency> appFileController)
        : base(services, logName: "Eav.ApiExplorer", connect: [user, appFileController])
    {
        _user = user;
        _appFileController = appFileController;

        ProvideOutRaw(GetApiFiles);
    }

    private IEnumerable<AppWebApiFileRaw> GetApiFiles()
    {
        var l = Log.Fn<IEnumerable<AppWebApiFileRaw>>($"list all api files a#{AppId}");

        var mask = $"*{EavConstants.ApiControllerSuffix}.cs";

        var localFiles = AppFileController.All(AppId, global: false, mask: mask, withSubfolders: true, returnFolders: false)
            .Select(file => new AppWebApiFileRaw
            {
                Path = file,
                EndpointPath = ApiFileEndpointPath(file),
                Edition = GetEdition(file),
            })
            .ToArray();

        l.A($"local files:{localFiles.Length}");

        var globalFiles = _user.IsSystemAdmin
            ? AppFileController.All(AppId, global: true, mask: mask, withSubfolders: true, returnFolders: false)
                .Select(file => new AppWebApiFileRaw
                {
                    Path = file,
                    Shared = true,
                    EndpointPath = ApiFileEndpointPath(file),
                    Edition = GetEdition(file),
                })
                .ToArray()
            : [];

        l.A($"global files:{globalFiles.Length}");

        var allInAppCode = AppFileController.AllApiFilesInAppCodeForAllEditions(AppId)
            .ToArray();

        l.A($"all in AppCode:{allInAppCode.Length}");

        var files = localFiles
            .Union(globalFiles)
            .Union(allInAppCode)
            .ToArray();

        var entities = files.Select((file, index) =>
        {
            file.Id = index + 1;
            return file;
        }).ToList();

        return l.Return(entities, $"{entities.Count}");
    }

    private IAppExplorerControllerDependency AppFileController => _appFileController.Value;

    private static string ApiFileEndpointPath(string relativePath)
        => AdjustControllerName(relativePath, $"{EavConstants.ApiControllerSuffix}.cs").ForwardSlash();

    public static string AppCodeEndpointPath(string edition, string controller)
        => Path.Combine(edition, EavConstants.Api, AdjustControllerName(controller, EavConstants.ApiControllerSuffix)).ForwardSlash();

    private static string AdjustControllerName(string controllerName, string suffix)
        => controllerName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
            ? controllerName.Substring(0, controllerName.Length - suffix.Length)
            : controllerName;

    private string GetEdition(string path)
    {
        var l = Log.Fn<string>($"{nameof(path)}:'{path}'");

        var edition = path.Split(['/'], StringSplitOptions.RemoveEmptyEntries)[0];

        return IsRootEdition(path, edition)
            ? l.Return(string.Empty, "edition: <root>")
            : l.Return(edition, $"ok, edition:'{edition}'");
    }

    private static bool IsRootEdition(string path, string edition)
        => edition.Equals(EavConstants.Api, StringComparison.OrdinalIgnoreCase)
           || edition.Equals(FolderConstants.AppCodeFolder, StringComparison.OrdinalIgnoreCase)
           || edition.Equals(FolderConstants.DataFolderProtected, StringComparison.OrdinalIgnoreCase)
           || edition.Equals(path, StringComparison.OrdinalIgnoreCase);
}
