using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Data.Build;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class StartupEavDataBuild
{
    public static IServiceCollection AddEavDataBuild(this IServiceCollection services)
    {
        services.TryAddTransient<IDataFactory, DataFactory>(); // v15.03
        services.TryAddTransient<DataBuilder>();

        // Content Type
        services.TryAddTransient<ContentTypeBuilder>();
        services.TryAddTransient<ContentTypeAttributeBuilder>();
        services.TryAddTransient<ContentTypeFactory>();

        // Entities
        services.TryAddTransient<DimensionBuilder>();
        services.TryAddTransient<AttributeBuilder>();
        services.TryAddTransient<EntityBuilder>();
        services.TryAddTransient<ValueBuilder>();

        return services;
    }

}