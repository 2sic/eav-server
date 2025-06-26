using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Testing.Scenarios;

namespace ToSic.Eav.Testing;

public static class TestFixturesStartup
{
    /// <summary>
    /// Add things necessary for DB fixtures to work.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddFixtureHelpers(this IServiceCollection services) =>
        services
            .AddTransient(typeof(DoFixtureStartup<>))
            .AddTransient<ScenarioBasic>()
            .AddTransient<FixtureStartupNoDb>()
            .AddTransient<FixtureStartupWithDb>();
}