using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Mock.LifetimeServices;

internal static class Setup
{
    public static IServiceCollection AddMockLifetimes(this IServiceCollection services)
    {
        services.TryAddTransient<MockTransientStandalone>();
        services.TryAddTransient<IMockTransientStandalone, MockTransientStandalone>();
        services.TryAddTransient<MockTransientRequiringTransient>();
        services.TryAddTransient<MockTransientRequiringScoped>();
        
        services.TryAddScoped<MockScopedStandalone>();
        services.TryAddScoped<MockScopedRequiringTransient>();
        services.TryAddScoped<MockScopedRequiringScoped>();
        services.TryAddScoped<MockScopedStandaloneToReRegister>();
        
        return services;
    }
}