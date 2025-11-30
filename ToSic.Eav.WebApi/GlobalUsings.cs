// Global using directives

global using System;
global using System.Collections.Generic;
global using System.Diagnostics.CodeAnalysis;
global using System.Text.Json.Serialization;
global using ToSic.Eav.Apps;
global using ToSic.Eav.Apps.Sys.Work;
global using ToSic.Eav.Data;
global using ToSic.Sys.DI;
global using ToSic.Sys.Documentation;
global using ToSic.Sys.Logging;
global using ToSic.Sys.Services;
global using ToSic.Sys.Performance;
global using ToSic.Sys.Utils;



#if NETFRAMEWORK
global using THttpResponseType = System.Net.Http.HttpResponseMessage;
#else
global using THttpResponseType = Microsoft.AspNetCore.Mvc.IActionResult;
#endif

#if NETFRAMEWORK
global using TDotNetController = System.Web.Http.ApiController;
#else
global using TDotNetController = Microsoft.AspNetCore.Mvc.ControllerBase;
#endif