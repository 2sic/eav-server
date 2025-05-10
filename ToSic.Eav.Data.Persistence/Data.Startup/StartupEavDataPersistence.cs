using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.Data.Startup;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupEavDataPersistence
{
    public static IServiceCollection AddEavDataPersistence(this IServiceCollection services)
    {
        services.TryAddTransient<EntitySaver>();
        
        return services;
    }


}