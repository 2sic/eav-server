﻿using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Eav.Testing;

public static class StartupHelpers
{
    /// <summary>
    /// Add things necessary for DB fixtures to work.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddFixtureHelpers(this IServiceCollection services) =>
        services
            .AddTransient<FullDbFixtureHelper>();
}