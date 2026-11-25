using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys.Run.Startup;

public static class StartupTestAccessors
{
    public static IServiceCollection AddSysCoreDiAllTac(this IServiceCollection services)
        => services.AddSysCoreDi();

    public static IServiceCollection AddSysCoreDiServiceProviderTac(this IServiceCollection services)
        => services.AddSysCoreDiServiceProvider();

    public static IServiceCollection AddSysCoreDiLazyAndGeneratorsTac(this IServiceCollection services)
        => services.AddSysCoreDiLazyAndGenerators();

    public static IServiceCollection AddSysCoreDiContextTac(this IServiceCollection services)
        => services.AddSysCoreDiContext();

    public static IServiceCollection AddSysCoreDiKeyRegistryTac(this IServiceCollection services)
        => services.AddSysCoreDiKeyRegistry();

    public static IServiceCollection AddSysCoreDiServiceSwitchersTac(this IServiceCollection services)
        => services.AddSysCoreDiServiceSwitchers();

}
