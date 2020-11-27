using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.LookUp;
using ToSic.Eav.Run;

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

        /// <summary>
        /// This will add Do-Nothing services which will take over if they are not provided by the main system
        /// In general this will result in some features missing, which many platforms don't need or care about
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        /// <remarks>
        /// All calls in here MUST use TryAddTransient, and never without the Try
        /// </remarks>
        public static IServiceCollection AddEavCoreFallbackServices(this IServiceCollection services)
        {
            services.TryAddTransient<IGetEngine, BasicGetLookupEngine>();
            services.TryAddTransient<IUser, UnknownUser>();
            services.TryAddTransient<IGetDefaultLanguage, BasicGetDefaultLanguage>();
            return services;
        }

    }
}
