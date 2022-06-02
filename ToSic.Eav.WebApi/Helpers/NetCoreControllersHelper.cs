#if !NETFRAMEWORK
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using ToSic.Eav.DI;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.WebApi.Helpers
{
    public class NetCoreControllersHelper
    {
        public NetCoreControllersHelper(Controller parent)
        {
            Parent = parent;
            LogOrNull = (parent as IHasLog)?.Log;
        }
        public Controller Parent { get; }
        public ILog LogOrNull { get; }

        private LogCall _actionTimerWrap; // it is used across events to track action execution total time

        public IServiceProvider ServiceProvider => _serviceProvider ?? throw new Exception($"{nameof(ServiceProvider)} is only available after calling {nameof(OnActionExecuting)}");
        private IServiceProvider _serviceProvider;


        public void OnActionExecuting(ActionExecutingContext context, string historyLogGroup)
        {
            // Create a log entry with timing
            _actionTimerWrap = LogOrNull.Fn($"action executing url: {context.HttpContext.Request.GetDisplayUrl()}", startTimer: true);

            // Get the ServiceProvider of the current request
            _serviceProvider = context.HttpContext.RequestServices;

            // Add to Log History
            _serviceProvider.Build<LogHistory>().Add(historyLogGroup, LogOrNull);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
            {
                // If the api endpoint method return type is "void" or "Task", Web API will return HTTP response with status code 204 (No Content).
                // This changes aspnetcore default behavior in Oqtane that returns HTTP 200 OK, with no body so it is same as in ASP.NET MVC2 in DNN. 
                // This is helpful for jQuery Ajax issue that on HTTP 200 OK with empty body throws json parse error.
                // https://docs.microsoft.com/en-us/aspnet/web-api/overview/getting-started-with-aspnet-web-api/action-results#void
                // https://github.com/dotnet/aspnetcore/issues/16944
                // https://github.com/2sic/2sxc/issues/2555
                var returnType = actionDescriptor.MethodInfo.ReturnType;
                if (returnType == typeof(void) || returnType == typeof(Task))
                {
                    if (context.HttpContext.Response.StatusCode == 200)
                        context.HttpContext.Response.StatusCode = 204; // NoContent (instead of HTTP 200 OK)
                }
            }

            _actionTimerWrap.Done("ok");
            _actionTimerWrap = null; // just to mark that Action Delegate is not in use any more, so GC can collect it
        }

        public TService GetService<TService>() => ServiceProvider.Build<TService>();

        public TRealController Real<TRealController>() where TRealController : class, IHasLog<TRealController>
            => ServiceProvider?.Build<TRealController>().Init(LogOrNull) ??
               throw new Exception($"Can't use {nameof(Real)} before {nameof(OnActionExecuting)}");
    }
}
#endif 