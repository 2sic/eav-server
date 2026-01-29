using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Data.Sys.PropertyDump;
using ToSic.Sys.Wrappers;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run.Startup;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
public static class StartupEavDataStack
{
    public static IServiceCollection AddEavDataStack(this IServiceCollection services)
    {
        return services
            .AddEavDataStackDumping();
    }


    public static IServiceCollection AddEavDataStackDumping(this IServiceCollection services)
    {
        services.TryAddTransient<IPropertyDumpService, PropertyDumpService>();
        services.AddTransient<IPropertyDumper, EntityDump>();
        services.AddTransient<IPropertyDumper, PropertyStackDump>();

        return services;
    }

}