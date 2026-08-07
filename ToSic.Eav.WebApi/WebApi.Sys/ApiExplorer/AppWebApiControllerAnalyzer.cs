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
        var controllerRaw = new AppWebApiControllerRaw
        {
            controller = controller.Name,
            ignoreSecurity = controllerSecurity.IgnoreSecurity,
            allowAnonymous = controllerSecurity.AllowAnonymous,
            requireVerificationToken = controllerSecurity.RequireVerificationToken,
            requireContext = controllerSecurity.RequireContext,
            view = controllerSecurity.View,
            edit = controllerSecurity.Edit,
            admin = controllerSecurity.Admin,
            superUser = controllerSecurity.SuperUser,
        };

        var endpoints = controller.GetMethods()
            .Where(methodInfo =>
                methodInfo.IsPublic &&
                !methodInfo.IsSpecialName &&
                inspector.GetHttpVerbs(methodInfo).Count > 0)
            .Select(methodInfo =>
            {
                var methodSecurity = inspector.GetSecurity(methodInfo);
                var mergedSecurity = ApiSecurityDto.MergeSecurity(controllerSecurity, methodSecurity, log: Log);

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
            security = security
                .ObjectToDictionary()
                .FilterOutKeys([
                    nameof(security.ValidateAntiForgeryToken),
                    nameof(security.AutoValidateAntiforgeryToken),
                    nameof(security.IgnoreAntiforgeryToken),
                ]),
            returns = returns,
            ignoreSecurity = mergedSecurity.IgnoreSecurity,
            allowAnonymous = mergedSecurity.AllowAnonymous,
            requireVerificationToken = mergedSecurity.RequireVerificationToken,
            requireContext = mergedSecurity.RequireContext,
            view = mergedSecurity.View,
            edit = mergedSecurity.Edit,
            admin = mergedSecurity.Admin,
            superUser = mergedSecurity.SuperUser,
        };
}
