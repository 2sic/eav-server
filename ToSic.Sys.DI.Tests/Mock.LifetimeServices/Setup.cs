using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.DI.ChildScope;

namespace ToSic.Mock.LifetimeServices;

internal static class Setup
{
    public static IServiceCollection AddMockLifetimes(this IServiceCollection services)
    {
        services.TryAddTransient<MockTransientStandalone>();
        services.TryAddOverrideableTransient<IMockTransientStandalone, MockTransientStandalone>();
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
    //public static IServiceCollection TryAddOverrideableTransient<TService>(this IServiceCollection services)
    //    where TService : class
    //{
    //    services.TryAddTransient<TService>(sp
    //        // 1. Check if an override is active in the current async context
    //        => OverrideContext<TService>.Current
    //           // 2. Fall back to the default implementation in the SAME scope
    //           ?? sp.GetRequiredService<TService>());
    //    return services;
    //}
    
    //public static IServiceCollection TryAddOverrideableTransient<TService, TImplementation>(this IServiceCollection services)
    //    where TService : class
    //    where TImplementation : class, TService
    //{
    //    services.TryAddTransient<TService>(sp
    //        // 1. Check if an override is active in the current async context
    //        => OverrideContext<TService>.Current
    //           // 2. Fall back to the default implementation in the SAME scope
    //           ?? sp.GetRequiredService<TImplementation>());
    //    return services;
    //}
    
    
    public static IServiceCollection TryAddOverrideableTransient<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.TryAddTransient<TService>(sp
            // 1. Check if an override is active in the current async context
            => OverrideContext<TService>.CurrentFactory is { } overrideFactory
                ? overrideFactory(sp)
                // 2. Fall back to the default implementation in the SAME scope
                : sp.GetRequiredService<TImplementation>()
        );
        return services;
    }
}