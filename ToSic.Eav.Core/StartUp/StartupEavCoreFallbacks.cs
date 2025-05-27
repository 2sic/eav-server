using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Catalog;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Internal.Compression;
using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Internal.Environment;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Internal.Requirements;
using ToSic.Eav.Internal.Unknown;
using ToSic.Eav.Security;
using ToSic.Eav.Security.Encryption;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Lib;
using ToSic.Lib.LookUp;
using CodeInfoService = ToSic.Sys.Code.InfoSystem.CodeInfoService;

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

        //services.TryAddTransient<ILookUpEngineResolver, LookUpEngineResolverUnknown>();
        services.AddLibLookUp();

        services.AddSysSecurity();
        //services.TryAddTransient<IUser, UserUnknown>();
        //services.TryAddTransient<IContextOfUserPermissions, ContextOfUserPermissions>();
        //services.TryAddTransient<IContextResolverUserPermissions, ContextResolverUserPermissions>();
        services.TryAddTransient<IZoneCultureResolver, ZoneCultureResolverUnknown>();
        services.TryAddTransient<IServerPaths, ServerPathsUnknown>();
        services.TryAddTransient<IAppContentTypesLoader, AppContentTypesLoaderUnknown>();

        // Special registration of iisUnknown to verify we see warnings if such a thing is loaded
        services.TryAddTransient<IIsUnknown, ServerPathsUnknown>();

        // Unknown-Runtime for loading configuration etc. File-runtime
        services.TryAddTransient<IAppLoader, AppLoaderUnknown>();
        services.TryAddTransient<IPlatformInfo, PlatformUnknown>();

        services.TryAddTransient<IRequirementsService, RequirementsServiceUnknown>();

        return services;
    }

}