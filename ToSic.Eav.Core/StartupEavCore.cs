using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Data.Builder;

namespace ToSic.Eav
{
    public static class StartupEavCore
    {
        public static IServiceCollection AddEavCore(this IServiceCollection services)
        {
            // todo
            services.TryAddTransient<AttributeBuilder>();

            return services;
        }

    }
}
