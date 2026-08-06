using System.Reflection;
using ToSic.Eav.Sys;
using ToSic.Eav.WebApi.Sys.Admin;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.ApiExplorer;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppWebApiControllerAnalyzer(IApiInspector inspector)
    : ServiceBase($"{EavLogs.WebApi}.ApiCtlAn", connect: [inspector])
{
    internal (AppWebApiControllerRaw Controller, IReadOnlyCollection<AppWebApiEndpointRaw> Endpoints) Analyze(string path, Assembly assembly)
    {
        var l = Log.Fn<(AppWebApiControllerRaw, IReadOnlyCollection<AppWebApiEndpointRaw>)>();

        path = CleanPath(path);
        var controller = GetController(path, assembly);
        var result = Build(controller);

        return l.ReturnAsOk(result);
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

    private (AppWebApiControllerRaw Controller, IReadOnlyCollection<AppWebApiEndpointRaw> Endpoints) Build(Type controller)
    {
        var l = Log.Fn<(AppWebApiControllerRaw, IReadOnlyCollection<AppWebApiEndpointRaw>)>();

        var controllerSecurity = inspector.GetSecurity(controller);
        var controllerRaw = ToControllerRaw(controller.Name, controllerSecurity);

        var endpoints = controller.GetMethods()
            .Where(methodInfo =>
                methodInfo.IsPublic &&
                !methodInfo.IsSpecialName &&
                inspector.GetHttpVerbs(methodInfo).Count > 0)
            .Select(methodInfo =>
            {
                var methodSecurity = inspector.GetSecurity(methodInfo);
                var mergedSecurity = MergeSecurity(controllerSecurity, methodSecurity);

                return ToEndpointRaw(
                    methodInfo.Name,
                    inspector.GetHttpVerbs(methodInfo),
                    methodInfo.GetParameters(),
                    ApiExplorerJs.JsTypeName(methodInfo.ReturnType),
                    methodSecurity,
                    mergedSecurity);
            })
            .ToArray();

        return l.ReturnAsOk((controllerRaw, endpoints));
    }

    private AppWebApiEndpointRaw ToEndpointRaw(
        string name,
        IEnumerable<string> verbs,
        IEnumerable<ParameterInfo> parameters,
        string returns,
        ApiSecurityDto security,
        ApiSecurityDto mergedSecurity)
        => new()
        {
            name = name,
            verbs = string.Join(", ", verbs.Select(verb => verb.ToUpperInvariant())),
            parameters = parameters
                .Select(parameter => new AppWebApiEndpointRaw.Parameter
                {
                    name = parameter.Name!,
                    type = ApiExplorerJs.JsTypeName(parameter.ParameterType),
                    defaultValue = parameter.DefaultValue,
                    isOptional = parameter.IsOptional,
                    isBody = inspector.IsBody(parameter),
                })
                .ToArray(),
            security = AppWebApiControllerSecurityValues.ToDictionary(security),
            returns = returns,
            IgnoreSecurity = mergedSecurity.IgnoreSecurity,
            AllowAnonymous = mergedSecurity.AllowAnonymous,
            RequireVerificationToken = mergedSecurity.RequireVerificationToken,
            RequireContext = mergedSecurity.RequireContext,
            View = mergedSecurity.View,
            Edit = mergedSecurity.Edit,
            Admin = mergedSecurity.Admin,
            SuperUser = mergedSecurity.SuperUser,
        };

    private static AppWebApiControllerRaw ToControllerRaw(string name, ApiSecurityDto security)
        => new()
        {
            controller = name,
            IgnoreSecurity = security.IgnoreSecurity,
            AllowAnonymous = security.AllowAnonymous,
            RequireVerificationToken = security.RequireVerificationToken,
            RequireContext = security.RequireContext,
            View = security.View,
            Edit = security.Edit,
            Admin = security.Admin,
            SuperUser = security.SuperUser,
        };

    private ApiSecurityDto MergeSecurity(ApiSecurityDto controllerSecurity, ApiSecurityDto methodSecurity)
    {
        var l = Log.Fn<ApiSecurityDto>();

        var ignoreSecurity = controllerSecurity.IgnoreSecurity || methodSecurity.IgnoreSecurity;
        var allowAnonymous = controllerSecurity.AllowAnonymous || methodSecurity.AllowAnonymous;
        var view = controllerSecurity.View || methodSecurity.View;
        var edit = controllerSecurity.Edit || methodSecurity.Edit;
        var admin = controllerSecurity.Admin || methodSecurity.Admin;
        var superUser = controllerSecurity.SuperUser || methodSecurity.SuperUser;
        var requireContext = controllerSecurity.RequireContext || methodSecurity.RequireContext;

        var requireVerificationToken =
            methodSecurity.ValidateAntiForgeryToken ||
            methodSecurity.AutoValidateAntiforgeryToken ||
            methodSecurity.IgnoreAntiforgeryToken
                ? methodSecurity.RequireVerificationToken
                : controllerSecurity.RequireVerificationToken;

        var result = new ApiSecurityDto
        {
            IgnoreSecurity = ignoreSecurity,
            AllowAnonymous = ignoreSecurity || allowAnonymous && !view && !edit && !admin && !superUser,
            View = ignoreSecurity || (allowAnonymous || view) && !edit && !admin && !superUser,
            Edit = ignoreSecurity || (allowAnonymous || view || edit) && !admin && !superUser,
            Admin = ignoreSecurity || (allowAnonymous || view || edit || admin) && !superUser,
            SuperUser = ignoreSecurity || allowAnonymous || view || edit || admin || superUser,
            RequireContext = !ignoreSecurity && requireContext,
            RequireVerificationToken = !ignoreSecurity && requireVerificationToken,
        };

        return l.ReturnAsOk(result);
    }
}
