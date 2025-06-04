using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Targets;

namespace ToSic.Eav.StartUp;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static partial class StartupEavData
{
    public static IServiceCollection AddEavData(this IServiceCollection services)
    {
        services.TryAddTransient<ITargetTypeService, TargetTypesService>();

        return services;
    }

}