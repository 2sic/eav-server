using Microsoft.Extensions.DependencyInjection;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;

namespace ToSic.Lib;

public static class StartUp
{
    public static IServiceCollection AddLibCore(this IServiceCollection services) =>
        services
            .AddLibLogging()
            .AddLibDI();
}