using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Security.Encryption;

// ReSharper disable once CheckNamespace
namespace ToSic.Sys.Run.Startup;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class StartupSysUtils
{
    public static IServiceCollection AddSysUtils(this IServiceCollection services)
    {

        services.TryAddTransient<AesCryptographyService>();
        services.TryAddTransient<Rfc2898Generator>();

        // v18
        services.TryAddTransient<RsaCryptographyService>();
        services.TryAddTransient<AesHybridCryptographyService>();

        return services;
    }

}