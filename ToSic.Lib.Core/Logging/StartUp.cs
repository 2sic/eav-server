using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Lib.Logging
{
    public static class StartUp
    {
        public static IServiceCollection AddLibLogging(this IServiceCollection services)
        {
            // History (very core service)
            services.TryAddTransient<ILogLiveHistory, History>();
            services.TryAddTransient<History>();

            return services;
        }
    }
}
