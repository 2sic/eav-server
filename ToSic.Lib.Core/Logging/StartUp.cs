using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Lib.Logging
{
    public static class StartUp
    {
        public static IServiceCollection AddLibLogging(this IServiceCollection services)
        {
            // History (very core service)
            services.TryAddTransient<ILogStoreLive, LogStoreLive>();
            services.TryAddTransient<ILogStore, LogStoreLive>();
            services.TryAddTransient<LogStoreLive>();

            return services;
        }
    }
}
