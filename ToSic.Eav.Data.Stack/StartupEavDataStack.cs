using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Data.Sys.PropertyDump;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Startup;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupEavDataStack
{
    public static IServiceCollection AddEavDataStack(this IServiceCollection services)
    {
        services.TryAddTransient<IPropertyDumpService, PropertyDumpService>();
        services.AddTransient<IPropertyDumper, EntityDump>();
        services.AddTransient<IPropertyDumper, PropertyStackDump>();

        return services;
    }

}