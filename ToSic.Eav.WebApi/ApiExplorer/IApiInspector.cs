using System.Collections.Generic;
using System.Reflection;
using ToSic.Lib.Logging;

namespace ToSic.Eav.WebApi.ApiExplorer
{
    public interface IApiInspector: IHasLog
    {
        bool IsBody(ParameterInfo paramInfo);

        List<string> GetHttpVerbs(MethodInfo methodInfo);
        
        ApiSecurityDto GetSecurity(MemberInfo member);
    }
}
