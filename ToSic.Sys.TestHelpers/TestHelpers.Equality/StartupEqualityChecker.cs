using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Sys.TestHelpers.Equality;

public static class StartupEqualityChecker
{
    public static IServiceCollection AddEqualityChecker(this IServiceCollection services)
    {
        services.TryAddTransient(typeof(EqualityChecker<>));
        return services;
    }

}
