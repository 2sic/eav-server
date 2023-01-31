using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Lib.Logging
{
    public static class StartUpLogging
    {
        public static IServiceCollection AddLibLogging(this IServiceCollection services)
        {
            // History (very core service)
            services.TryAddTransient<ILogStore, LogStoreLive>();
            services.TryAddTransient<ILogStoreLive, LogStoreLive>();

            return services;
        }
    }
}
