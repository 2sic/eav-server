using System.Collections.Generic;
using System.Reflection;
using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.WebApi.ApiExplorer;

internal class ApiInspectorUnknown: ServiceBase, IApiInspector
{
    public ApiInspectorUnknown(WarnUseOfUnknown<ApiInspectorUnknown> _) : base($"{LogScopes.NotImplemented}.ApiIns")
    {
    }

    public bool IsBody(ParameterInfo paramInfo) => false;

    public List<string> GetHttpVerbs(MethodInfo methodInfo) => new();

    public ApiSecurityDto GetSecurity(MemberInfo member) => new();
}