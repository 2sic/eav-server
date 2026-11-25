using System.Reflection;
using ToSic.Eav.Apps.Inspect.Sys.WebApi.Dto;

namespace ToSic.Eav.Apps.Inspect.Sys.WebApi;

public interface IApiInspector: IHasLog
{
    bool IsBody(ParameterInfo paramInfo);

    List<string> GetHttpVerbs(MethodInfo methodInfo);
        
    ApiSecurityDto GetSecurity(MemberInfo member);
}