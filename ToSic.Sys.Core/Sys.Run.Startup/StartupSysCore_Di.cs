using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.DI;
using ToSic.Sys.Services;

namespace ToSic.Sys.Run.Startup;

public static partial class StartupSysCore
{
    /// <summary>
    /// Add core Dependency Injection prats, such as Lazy, Generator, etc.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSysCoreDi(this IServiceCollection services)
    {
        // Lazy objects in General
        services.TryAddTransient(typeof(Lazy<>), typeof(LazyImplementation<>));

        // Lazy Services
        services.TryAddTransient(typeof(LazySvc<>));

        // Service Generators
        services.TryAddTransient(typeof(Generator<>));
        services.TryAddTransient(typeof(Generator<,>));

        // Warnings for mock implementations
        services.TryAddTransient(typeof(WarnUseOfUnknown<>));

        // Empty MyServices
        services.TryAddTransient<DependenciesEmpty>();

        return services;
    }
}
