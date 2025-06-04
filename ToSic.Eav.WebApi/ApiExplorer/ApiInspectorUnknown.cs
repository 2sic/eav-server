using System.Reflection;

namespace ToSic.Eav.WebApi.ApiExplorer;

internal class ApiInspectorUnknown: ServiceBase, IApiInspector
{
    public ApiInspectorUnknown(WarnUseOfUnknown<ApiInspectorUnknown> _) : base($"{LogScopes.NotImplemented}.ApiIns")
    {
    }

    public bool IsBody(ParameterInfo paramInfo) => false;

    public List<string> GetHttpVerbs(MethodInfo methodInfo) => [];

    public ApiSecurityDto GetSecurity(MemberInfo member) => new();
}