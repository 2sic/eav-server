using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.DI.ChildScope;

namespace ToSic.Mock.LifetimeServices;

internal static class Setup
{
    public static IServiceCollection AddMockLifetimes(this IServiceCollection services)
    {
        services.TryAddTransient<MockTransientStandalone>();
        services.TryAddTransient(OverrideContext<IMockTransientStandalone>.Register<MockTransientStandalone>());
        services.TryAddTransient<MockTransientRequiringTransient>();
        services.TryAddTransient<MockTransientRequiringScoped>();


        services.TryAddScoped<MockScopedStandalone>();
        services.TryAddScoped<MockScopedRequiringTransient>();
        services.TryAddScoped<MockScopedRequiringScoped>();
        services.TryAddScoped<MockScopedToReRegisterReqITransient>();
        
        return services;
    }

    public static IServiceCollection AddMockPreRegisterChildInstances(this IServiceCollection services)
    {
        // Register the MockChildScopeOnlyTransientPreRegistered as a transient service
        // we'll later access it through the interface, but
        services.TryAddTransient<MockChildScopeOnlyTransientPreRegistered>();
        return services;
    }
}

public static class ServiceCollectionExtensions
{
    
    public static IServiceCollection AddOverrideableScoped<TService>(this IServiceCollection services)
        where TService : class
        => services.AddScoped(OverrideContext<TService>.Register<TService>());

    public static IServiceCollection AddOverrideableScoped<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
        => services.AddScoped(OverrideContext<TService>.Register<TImplementation>());
    
    public static IServiceCollection AddOverrideableTransient<TService>(this IServiceCollection services)
        where TService : class
        => services.AddTransient(OverrideContext<TService>.Register<TService>());

    public static IServiceCollection AddOverrideableTransient<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
        => services.AddTransient(OverrideContext<TService>.Register<TImplementation>());

    public static IServiceCollection TryAddOverrideableTransient<TService>(this IServiceCollection services)
        where TService : class
    {
        services.TryAddTransient(OverrideContext<TService>.Register<TService>());
        return services;
    }
    
    public static IServiceCollection TryAddOverrideableTransient<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.TryAddTransient(OverrideContext<TService>.Register<TImplementation>());
        return services;
    }

    ///// <summary>
    ///// Generate the factory function to either get from the override, or from the underlying service provider.
    ///// </summary>
    ///// <typeparam name="TService">Service type or interface</typeparam>
    ///// <typeparam name="TImplementation">Implementation type</typeparam>
    ///// <returns>A factory function to create the service instance</returns>
    //private static Func<IServiceProvider, TService> Overrideable<TService, TImplementation>()
    //    where TService : class
    //    where TImplementation : class, TService
    //{
    //    // Check if an override is active in the current async context
    //    return sp => OverrideContext<TService>.CurrentFactory is { } overrideFactory
    //        // 1. Return from new factory
    //        ? overrideFactory(sp)
    //        // 2. Fall back to the default implementation in the SAME scope
    //        : sp.GetRequiredService<TImplementation>();
    //}
}