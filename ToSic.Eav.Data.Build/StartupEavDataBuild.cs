using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Build.Sys;
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
        services.TryAddTransient<DataAssembler>();

        // Content Type
        services.TryAddTransient<ContentTypeTypeAssembler>();
        services.TryAddTransient<ContentTypeAttributeAssembler>();
        services.TryAddTransient<ContentTypeAssembler>();
        services.TryAddTransient<CodeContentTypesManager>();

        // Entities
        services.TryAddTransient<LanguageAssembler>();
        services.TryAddTransient<AttributeAssembler>();
        services.TryAddTransient<AttributeListAssembler>();
        services.TryAddTransient<EntityAssembler>();
        services.TryAddTransient<EntityConnectionBuilder>();
        services.TryAddTransient<ValueAssembler>();
        services.TryAddTransient<ValueListAssembler>();
        services.TryAddTransient<RelationshipAssembler>();

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