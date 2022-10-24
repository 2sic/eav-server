using System.Collections.Generic;
using System.Reflection;
using ToSic.Lib.Logging;
using ToSic.Eav.Run.Unknown;

namespace ToSic.Eav.WebApi.ApiExplorer
{
    public class ApiInspectorUnknown: HasLog<IApiInspector>, IApiInspector
    {
        public ApiInspectorUnknown(WarnUseOfUnknown<ApiInspectorUnknown> warn) : base($"{LogNames.NotImplemented}.ApiIns")
        {
        }

        public bool IsBody(ParameterInfo paramInfo) => false;

        public List<string> GetHttpVerbs(MethodInfo methodInfo) => new List<string>();

        public ApiSecurityDto GetSecurity(MemberInfo member) => new ApiSecurityDto();
    }
}
