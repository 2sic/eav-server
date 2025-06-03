using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Entities.Sys;
using ToSic.Eav.Data.PropertyDump.Sys;

namespace ToSic.Eav.StartUp;

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