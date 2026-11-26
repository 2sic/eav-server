using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Sys.Run.Startup;

public static partial class StartupSysCore
{
    /// <summary>
    /// Add **all** core Dependency Injection parts, such as Lazy, Generator, etc.
    /// </summary>
    /// <remarks>
    /// For testing or special integrations,
    /// you may want to explicitly add only the parts you need,
    /// using the other methods in this class.
    ///
    /// But since everything uses `TryAdd`, it's also safe to just add them before calling this.
    /// </remarks>
    public static IServiceCollection AddSysCoreDi(this IServiceCollection services)
    {
        // Foundation. This is absolutely necessary, as it registers the ISysServiceProvider
        // which is used everywhere.
        services.AddSysCoreDiServiceProvider();

        services.AddSysCoreDiLazyAndGenerators();

        // Warnings for mock implementations
        services.TryAddTransient(typeof(WarnUseOfUnknown<>));

        // New context aware DI - v22.02
        services.AddSysCoreDiContext();

        // New keyed service registry v22.02
        services.AddSysCoreDiKeyRegistry();

        return services;
    }

    /// <summary>
    /// Add Dependency Injection foundation - needed for almost everything.
    /// </summary>
    /// <remarks>
    /// Adds
    /// 
    /// 1. <see cref="ISysServiceProvider"/> - the context aware service provider
    /// </remarks>
    public static IServiceCollection AddSysCoreDiServiceProvider(this IServiceCollection services)
    {
        // Special Service Provider which is context aware
        // This is necessary, because IServiceProvider would always revert to the original one
        // which is not aware of the context.
        // This eliminates the need for rewriting all the registrations.
        services.TryAddTransient<ISysServiceProvider, ContextualServiceProvider>();
        return services;
    }

    /// <summary>
    /// Add Dependency Injection foundation - needed for almost everything.
    /// </summary>
    /// <remarks>
    /// Adds
    /// 
    /// 1. <see cref="Lazy{T}"/> - lazy objects
    /// 2. <see cref="LazySvc{T}"/> - lazy services, preferred method for services as it connects logs
    /// 3. <see cref="Generator{T}"/> - classic generator
    /// 4. <see cref="Generator{T, TOptions}"/> - generator with options
    /// </remarks>
    public static IServiceCollection AddSysCoreDiLazyAndGenerators(this IServiceCollection services)
    {
        // Lazy objects in General. Should be avoided, but added in case it's accidentally used and for tests
        services.TryAddTransient(typeof(Lazy<>), typeof(LazyImplementation<>));

        // Lazy Services
        services.TryAddTransient(typeof(LazySvc<>));

        // Service Generators
        services.TryAddTransient(typeof(Generator<>));      // Classic generator
        services.TryAddTransient(typeof(Generator<,>));     // Generator with Options
        return services;
    }

    /// <summary>
    /// Add Dependency Injection context services - used mostly in code which is aware of the context.
    /// </summary>
    /// <remarks>
    /// Adds
    /// 
    /// 2. <see cref="Ctx{T}"/> - context specifically injected objects
    /// 3. <see cref="CtxValue{T}"/> - context specifically injected values (internal)
    /// </remarks>
    public static IServiceCollection AddSysCoreDiContext(this IServiceCollection services)
    {
        // Context Services
        // Register an empty Ctx so that asking for it in DI doesn't throw an error
        // It will still not work unless the specific Ctx is registered,
        // but it will allow for clearer error messages to indicate missing registration.
        services.TryAddTransient(typeof(Ctx<>));
        services.TryAddTransient(typeof(CtxValue<>));
        return services;
    }

    /// <summary>
    /// Register the system to pick up all keys of all registered, named services.
    /// </summary>
    /// <remarks>
    /// Call once anywhere in your startup configuration—order does not matter.
    /// Adds:
    ///
    /// 1. <see cref="KeyedServiceRegistry"/> - the registry itself
    /// 2. <see cref="IAllKeyedServices{TKey, TService}"/> - the resolver/lookup
    /// </remarks>
    public static IServiceCollection AddSysCoreDiKeyRegistry(this IServiceCollection services)
    {
        // Add the registry itself
        services.TryAddSingleton(new KeyedServiceRegistry(services));

        // Add the resolver/lookup
        services.TryAddTransient(typeof(IAllKeyedServices<,>), typeof(AllKeyedServices<,>));
        return services;
    }

    /// <summary>
    /// Add Service Switchers OBSOLETE - v22.02
    /// </summary>
    public static IServiceCollection AddSysCoreDiServiceSwitchers(this IServiceCollection services)
    {
        // v22 - removing these; wait for AppsCache to be updated to remove all
        // #DropServiceSwitcherV22
        //services.TryAddTransient(typeof(ServiceSwitcher<>));
        //services.TryAddScoped(typeof(ServiceSwitcherScoped<>)); // note: it's for scoped, and we must use another object name here
        //services.TryAddTransient(typeof(ServiceSwitcherSingleton<>)); // note: it's for singletons, but the service is transient on purpose!
        return services;
    }
}