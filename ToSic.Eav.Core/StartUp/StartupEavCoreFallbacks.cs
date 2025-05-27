using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Internal.Environment;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Internal.Requirements;
using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.LookUp;
using ToSic.Sys;

namespace ToSic.Eav.StartUp;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartUpEavCoreFallbacks
{

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
        // Warnings for mock implementations
        services.TryAddTransient(typeof(WarnUseOfUnknown<>));

        // very basic stuff - normally overriden by the platform
        services.TryAddTransient<IValueConverter, ValueConverterUnknown>();

        services.AddLibLookUp();

        services.AddSysSecurity();
        services.TryAddTransient<IZoneCultureResolver, ZoneCultureResolverUnknown>();
        services.TryAddTransient<IServerPaths, ServerPathsUnknown>();
        services.TryAddTransient<IAppContentTypesLoader, AppContentTypesLoaderUnknown>();

        // Special registration of iisUnknown to verify we see warnings if such a thing is loaded
        services.TryAddTransient<IIsUnknown, ServerPathsUnknown>();

        // Unknown-Runtime for loading configuration etc. File-runtime
        services.TryAddTransient<IAppLoader, AppLoaderUnknown>();

        services.TryAddTransient<IRequirementsService, RequirementsServiceUnknown>();

        return services;
    }

}