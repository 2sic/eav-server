using System.IO;
using System.Reflection;
using ToSic.Eav.Context;
using ToSic.Eav.Helpers;
using ToSic.Eav.WebApi.Infrastructure;
#if NETFRAMEWORK
using THttpResponseType = System.Net.Http.HttpResponseMessage;
#else
using THttpResponseType = Microsoft.AspNetCore.Mvc.IActionResult;
#endif

namespace ToSic.Eav.WebApi.ApiExplorer;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ApiExplorerControllerReal(IUser user, IApiInspector inspector, IResponseMaker responseMaker, LazySvc<Admin.IAppExplorerControllerDependency> appFileController)
    : ServiceBase($"{EavLogs.WebApi}.{LogSuffix}Rl", connect: [inspector, responseMaker, appFileController])
{
    public const string LogSuffix = "ApiExp";

    public IApiInspector Inspector { get; } = inspector;
    public IResponseMaker ResponseMaker { get; } = responseMaker;

    public THttpResponseType Inspect(string path, Func<string, Assembly> getAssembly)
    {
        var l = Log.Fn<THttpResponseType>();
        if (PreCheckAndCleanPath(ref path, out var error))
            return l.Return(error, "error");

        try
        {
            return l.ReturnAsOk(AnalyzeClassAndCreateDto(path, getAssembly(path)));
        }
        catch (Exception exc)
        {
            l.Ex(exc);
            return l.Return(ResponseMaker.InternalServerError(exc), $"Error: {exc.Message}.");
        }
    }

    private bool PreCheckAndCleanPath(ref string path, out THttpResponseType error)
    {
        var wrapLog = Log.Fn<bool>();

        Log.A($"Controller Path from appRoot: {path}");

        if (string.IsNullOrWhiteSpace(path) || path.Contains(".."))
        {
            var msg = $"Error: bad parameter {path}";
            {
                error = ResponseMaker.InternalServerError(msg);
                return wrapLog.ReturnTrue(msg);
            }
        }

        // Ensure make windows path slashes to make later work easier
        path = path.Backslash();
        error = default; // null
        return wrapLog.ReturnFalse();
    }

    private THttpResponseType AnalyzeClassAndCreateDto(string path, Assembly assembly)
    {
        var wrapLog = Log.Fn<THttpResponseType>();
        var controllerName = path.Substring(path.LastIndexOf('\\') + 1);
        controllerName = controllerName.Substring(0, controllerName.IndexOf('.'));
        var controller =
            assembly.DefinedTypes.FirstOrDefault(a =>
                controllerName.Equals(a.Name, StringComparison.InvariantCultureIgnoreCase));
        if (controller == null)
        {
            var msg =
                $"Error: can't find controller class: {controllerName} in file {Path.GetFileNameWithoutExtension(path)}. " +
                $"This can happen if the controller class does not have the same name as the file.";
                return wrapLog.Return(ResponseMaker.InternalServerError(msg), "error");
        }

        var controllerDto = BuildApiControllerDto(controller);

        var responseMessage = ResponseMaker.Json(controllerDto);
        return wrapLog.ReturnAsOk(responseMessage);
    }

    private ApiControllerDto BuildApiControllerDto(Type controller)
    {
        var l = Log.Fn<ApiControllerDto>();
        var controllerSecurity = Inspector.GetSecurity(controller);
        var controllerDto = new ApiControllerDto
        {
            controller = controller.Name,
            actions = controller.GetMethods()
                .Where(methodInfo => methodInfo.IsPublic
                                     && !methodInfo.IsSpecialName
                                     && Inspector.GetHttpVerbs(methodInfo).Count > 0)
                .Select(methodInfo =>
                {
                    var methodSecurity = Inspector.GetSecurity(methodInfo);
                    var mergedSecurity = MergeSecurity(controllerSecurity, methodSecurity);
                    return new ApiActionDto
                    {
                        name = methodInfo.Name,
                        verbs = Inspector.GetHttpVerbs(methodInfo).Select(m => m.ToUpperInvariant()),
                        parameters = methodInfo.GetParameters().Select(p => new ApiActionParamDto
                        {
                            name = p.Name,
                            type = ApiExplorerJs.JsTypeName(p.ParameterType),
                            defaultValue = p.DefaultValue,
                            isOptional = p.IsOptional,
                            isBody = Inspector.IsBody(p),
                        }).ToArray(),
                        security = methodSecurity,
                        mergedSecurity = mergedSecurity,
                        returns = ApiExplorerJs.JsTypeName(methodInfo.ReturnType),
                    };
                }),
            security = controllerSecurity
        };
        return l.ReturnAsOk(controllerDto);
    }

    private ApiSecurityDto MergeSecurity(ApiSecurityDto contSec, ApiSecurityDto methSec)
    {
        var wrapLog = Log.Fn<ApiSecurityDto>();
        var ignoreSecurity = contSec.ignoreSecurity || methSec.ignoreSecurity;
        var allowAnonymous = contSec.allowAnonymous || methSec.allowAnonymous;
        var view = contSec.view || methSec.view;
        var edit = contSec.edit || methSec.edit;
        var admin = contSec.admin || methSec.admin;
        var superUser = contSec.superUser || methSec.superUser;
        var requireContext = contSec.requireContext || methSec.requireContext;
        // AntiForgeryToken attributes on method prevails over attributes on class (last attribute wins)
        var requireVerificationToken =
            (methSec._validateAntiForgeryToken || methSec._autoValidateAntiforgeryToken ||
             methSec._ignoreAntiforgeryToken)
                ? methSec.requireVerificationToken
                : contSec.requireVerificationToken;

        var result = new ApiSecurityDto
        {
            ignoreSecurity = ignoreSecurity,
            allowAnonymous = ignoreSecurity || allowAnonymous && !view && !edit && !admin && !superUser,
            view = ignoreSecurity || (allowAnonymous || view) && !edit && !admin && !superUser,
            edit = ignoreSecurity || (allowAnonymous || view || edit) && !admin && !superUser,
            admin = ignoreSecurity || (allowAnonymous || view || edit || admin) && !superUser,
            superUser = ignoreSecurity || allowAnonymous || view || edit || admin || superUser,
            requireContext = !ignoreSecurity && requireContext,
            requireVerificationToken = !ignoreSecurity && requireVerificationToken,
        };
        return wrapLog.ReturnAsOk(result);
    }

    public AllApiFilesDto AppApiFiles(int appId)
    {
        var l = Log.Fn<AllApiFilesDto>($"list all api files a#{appId}");

        var mask = $"*{Constants.ApiControllerSuffix}.cs";

        var localFiles =
            appFileController.Value.All(appId, global: false, mask: mask, withSubfolders: true, returnFolders: false)
                .Select(f => new AllApiFileDto { Path = f, EndpointPath = ApiFileEndpointPath(f) }).ToArray();
        l.A($"local files:{localFiles.Length}");

        var globalFiles = user.IsSystemAdmin
            ? appFileController.Value.All(appId, global: true, mask: mask, withSubfolders: true, returnFolders: false)
                .Select(f => new AllApiFileDto { Path = f, Shared = true, EndpointPath = ApiFileEndpointPath(f) }).ToArray()
            : [];
        l.A($"global files:{globalFiles.Length}");

        // only for api controller files
        var allInAppCode = appFileController.Value.AllApiFilesInAppCodeForAllEditions(appId).ToArray();
        l.A($"all in AppCode:{allInAppCode.Length}");

        return l.ReturnAsOk(new() { Files = localFiles.Union(globalFiles).Union(allInAppCode) });
    }

    private static string ApiFileEndpointPath(string relativePath)
        => AdjustControllerName(relativePath, $"{Constants.ApiControllerSuffix}.cs").ForwardSlash().PrefixSlash();

    public static string AppCodeEndpointPath(string edition, string controller)
        => Path.Combine(edition, Constants.Api, AdjustControllerName(controller, Constants.ApiControllerSuffix)).ForwardSlash().PrefixSlash();

    private static string AdjustControllerName(string controllerName, string suffix)
        => controllerName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
            ? controllerName.Substring(0, controllerName.Length - suffix.Length)
            : controllerName;
}

