using System.Reflection;
using ToSic.Eav.Sys;
using ToSic.Eav.WebApi.Sys.Helpers.Http;
using ToSic.Sys.Utils;

namespace ToSic.Eav.WebApi.Sys.ApiExplorer;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppWebApiControllerAnalyzer(IApiInspector inspector)
    : ServiceBase($"{EavLogs.WebApi}.ApiCtlAn", connect: [inspector])
{
    internal ApiControllerDto Analyze(string path, Assembly assembly)
    {
        var l = Log.Fn<ApiControllerDto>();

        path = CleanPath(path);
        var controller = GetController(path, assembly);
        var dto = BuildApiControllerDto(controller);

        return l.ReturnAsOk(dto);
    }

    private string CleanPath(string path)
    {
        var l = Log.Fn<string>();

        l.A($"Controller Path from appRoot: {path}");

        if (string.IsNullOrWhiteSpace(path) || path.Contains(".."))
            throw new($"Error: bad parameter {path}");

        return l.Return(path.Backslash(), "ok");
    }

    private Type GetController(string path, Assembly assembly)
    {
        var l = Log.Fn<Type>();

        var controllerName = path.Substring(path.LastIndexOf('\\') + 1);
        controllerName = controllerName.Substring(0, controllerName.IndexOf('.'));

        var controller = assembly.DefinedTypes.FirstOrDefault(type =>
            controllerName.Equals(type.Name, StringComparison.InvariantCultureIgnoreCase));

        if (controller == null)
            throw new($"Error: can't find controller class: {controllerName} in file {Path.GetFileNameWithoutExtension(path)}. " +
                      "This can happen if the controller class does not have the same name as the file.");

        return l.ReturnAsOk(controller);
    }

    private ApiControllerDto BuildApiControllerDto(Type controller)
    {
        var l = Log.Fn<ApiControllerDto>();

        var controllerSecurity = inspector.GetSecurity(controller);

        var controllerDto = new ApiControllerDto
        {
            controller = controller.Name,
            actions = controller.GetMethods()
                .Where(methodInfo =>
                    methodInfo.IsPublic &&
                    !methodInfo.IsSpecialName &&
                    inspector.GetHttpVerbs(methodInfo).Count > 0)
                .Select(methodInfo =>
                {
                    var methodSecurity = inspector.GetSecurity(methodInfo);
                    var mergedSecurity = MergeSecurity(controllerSecurity, methodSecurity);

                    return new ApiActionDto
                    {
                        name = methodInfo.Name,
                        verbs = inspector.GetHttpVerbs(methodInfo).Select(verb => verb.ToUpperInvariant()),
                        parameters = methodInfo.GetParameters()
                            .Select(parameter => new ApiActionParamDto
                            {
                                name = parameter.Name!,
                                type = ApiExplorerJs.JsTypeName(parameter.ParameterType),
                                defaultValue = parameter.DefaultValue,
                                isOptional = parameter.IsOptional,
                                isBody = inspector.IsBody(parameter),
                            })
                            .ToArray(),
                        security = methodSecurity,
                        mergedSecurity = mergedSecurity,
                        returns = ApiExplorerJs.JsTypeName(methodInfo.ReturnType),
                    };
                }),
            security = controllerSecurity
        };

        return l.ReturnAsOk(controllerDto);
    }

    private ApiSecurityDto MergeSecurity(ApiSecurityDto controllerSecurity, ApiSecurityDto methodSecurity)
    {
        var l = Log.Fn<ApiSecurityDto>();

        var ignoreSecurity = controllerSecurity.ignoreSecurity || methodSecurity.ignoreSecurity;
        var allowAnonymous = controllerSecurity.allowAnonymous || methodSecurity.allowAnonymous;
        var view = controllerSecurity.view || methodSecurity.view;
        var edit = controllerSecurity.edit || methodSecurity.edit;
        var admin = controllerSecurity.admin || methodSecurity.admin;
        var superUser = controllerSecurity.superUser || methodSecurity.superUser;
        var requireContext = controllerSecurity.requireContext || methodSecurity.requireContext;

        var requireVerificationToken =
            methodSecurity._validateAntiForgeryToken ||
            methodSecurity._autoValidateAntiforgeryToken ||
            methodSecurity._ignoreAntiforgeryToken
                ? methodSecurity.requireVerificationToken
                : controllerSecurity.requireVerificationToken;

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

        return l.ReturnAsOk(result);
    }
}