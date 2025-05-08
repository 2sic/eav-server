using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.Data.Startup;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class StartupEavDataPersistence
{
    public static IServiceCollection AddEavDataPersistence(this IServiceCollection services)
    {
        services.TryAddTransient<EntitySaver>();
        
        return services;
    }


}