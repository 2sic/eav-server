using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ToSic.Eav.Helpers;
using ToSic.Eav.WebApi.Infrastructure;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;
#if NETFRAMEWORK
using THttpResponseType = System.Net.Http.HttpResponseMessage;
#else
using THttpResponseType = Microsoft.AspNetCore.Mvc.IActionResult;
#endif

namespace ToSic.Eav.WebApi.ApiExplorer
{
    public class ApiExplorerControllerReal : ServiceBase
    {
        public const string LogSuffix = "ApiExp";

        public IApiInspector Inspector { get; }
        public IResponseMaker ResponseMaker { get; }

        public ApiExplorerControllerReal(IApiInspector inspector, IResponseMaker responseMaker): base($"{EavLogs.WebApi}.{LogSuffix}Rl")
        {
            Inspector = inspector;
            ResponseMaker = responseMaker;
        }

        public THttpResponseType Inspect(string path, Func<string, Assembly> getAssembly) => Log.Func(() =>
        {
            if (PreCheckAndCleanPath(ref path, out var error))
                return (error, "error");

            try
            {
                return (AnalyzeClassAndCreateDto(path, getAssembly(path)), "ok");
            }
            catch (Exception exc)
            {
                return (ResponseMaker.InternalServerError(exc), $"Error: {exc.Message}.");
            }
        });

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

        private ApiControllerDto BuildApiControllerDto(Type controller) => Log.Func(() =>
        {
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
            return controllerDto;
        });

        private ApiSecurityDto MergeSecurity(ApiSecurityDto contSec, ApiSecurityDto methSec) => Log.Func(() =>
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
            return result;
        });
    }
}
