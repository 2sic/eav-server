using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Processing;
using ToSic.Eav.Data.Sys.ValueConverter;
using ToSic.Eav.Metadata.Sys;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run.Startup;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
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
        services.TryAddTransient<EntityConnectionBuilder>();
        services.TryAddTransient<ValueBuilder>();

        services.AddEavDataProcessors();

        return services;
    }

    /// <summary>
    /// Data Processors v21 WIP
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddEavDataProcessors(this IServiceCollection services)
    {
        // Register using the interface, so it can be found in dropdowns
        // Must be "Add", since many should be possible.
        services.AddTransient<IDataProcessor, DataProcessor>();
        services.AddTransient<IDataProcessor, PermissionDataProcessor>();

        // Register directly, so it can be instantiated
        services.TryAddTransient<PermissionDataProcessor>();

        return services;
    }

    public static IServiceCollection AddEavDataBuildFallbacks(this IServiceCollection services)
    {
        // very basic stuff - normally overriden by the platform
        services.TryAddTransient<IValueConverter, ValueConverterUnknown>();

        return services;
    }

}