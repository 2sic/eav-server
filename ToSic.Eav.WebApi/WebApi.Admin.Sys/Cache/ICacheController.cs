
#if NETFRAMEWORK
using System.Web.Http;
using FromRoute = System.Web.Http.FromUriAttribute;
#else
using Microsoft.AspNetCore.Mvc;
#endif

namespace ToSic.Eav.WebApi.Admin.Sys;

public interface ICacheController: IAppAdminWebApiController
{
    bool Flush([FromBody] AppCacheFlushSpecs specs, [FromRoute] int? appId = null);
    bool Flush([FromRoute] string appPath, [FromBody] AppCacheFlushSpecs specs);
}
