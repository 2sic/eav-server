
#if NETFRAMEWORK
using System.Web.Http;
using FromRoute = System.Web.Http.FromUriAttribute;
#else
using Microsoft.AspNetCore.Mvc;
#endif

namespace ToSic.Eav.WebApi.Admin.Sys;

public interface ICacheController: IAppAdminWebApiController
{
    // UI test: Not called by eav-ui; exercise admin/cache/flush with an appId directly.
    bool Flush([FromBody] AppCacheFlushSpecs specs, [FromRoute] int? appId = null);
    // UI test: Not called by eav-ui; exercise admin/cache/flush with an appPath directly.
    bool Flush([FromRoute] string appPath, [FromBody] AppCacheFlushSpecs specs);
}
