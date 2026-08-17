using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.DI;
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

        
        // Note: any scoped which we would ever want to override
        // must be registered as transient, and just created once.
        // so we could have the implementation as scoped
        //services.TryAddScoped(OverrideContext<IMockScopedToReRegisterReqITransient>.Register<MockScopedToReRegisterReqITransient>());
        services.TryAddTransient<MockScopedToReRegisterReqITransient>();
        services.TryAddTransient(
            OverrideContext<IMockScopedToReRegisterReqITransient>.RegisterScoped<MockScopedToReRegisterReqITransient>());
        
        services.TryAddTransient<MockChildScopeOnlyTransientBasic>();
        
        return services;
    }

    public static IServiceCollection AddMockPreRegisterChildInstances(this IServiceCollection services)
    {
        // Register the MockChildScopeOnlyTransientPreRegistered as a transient service
        // we'll later access it through the interface, but
        services.TryAddTransient<MockChildScopeOnlyTransientPreRegistered>();
        return services;
    }
    
    public static IServiceCollection AddMockInitialsWhichThrow(this IServiceCollection services)
    {
        // Register the MockChildScopeOnlyTransientPreRegistered as a transient service
        // we'll later access it through the interface, but
        services.TryAddTransient(OverrideContext<IMockChildScopeOnlyScopedBasic>.Register(() => new MockChildScopeOnlyScopedBasicInitialThrows()));
        services.TryAddTransient<MockChildScopeOnlyScopedBasic>();  // make sure the underlying real implementation can be resolved
        return services;
    }
    
}
