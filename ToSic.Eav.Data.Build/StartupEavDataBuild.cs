using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Processing;
using ToSic.Eav.Data.Sys.ValueConverter;
using ToSic.Eav.Metadata.Sys;
using ToSic.Sys.Utils.Assemblies;

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
        // Register baseline processors using interface so they are discoverable in the system data-source.
        // Dedupe-safe multi-registration (safe if called multiple times)
        // Microsoft DI has this pattern specifically for multi-registrations that should not duplicate: TryAddEnumerable.
        // - It still allows multiple implementations of the same service (which you want for IDataProcessor).
        // - But it prevents duplicates of the exact same (service type + implementation type) from being added again.
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IDataProcessor), typeof(DataProcessor)));
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IDataProcessor), typeof(PermissionDataProcessor)));

        // Register directly, so it can be instantiated
        services.TryAddTransient<PermissionDataProcessor>();

        // Auto-register all concrete IDataProcessor implementations from loaded assemblies (incl. optional bin dlls).
        // Keep this startup-only and dedupe by stable identity to avoid duplicate service descriptors.
        var discoveredTypes = AssemblyHandling
            .FindInherited(typeof(IDataProcessor))
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .GroupBy(type => type.FullName ?? type.AssemblyQualifiedName ?? type.Name, StringComparer.Ordinal)
            .Select(group => group.First());

        foreach (var discoveredType in discoveredTypes)
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IDataProcessor), discoveredType));

            // Register concrete type too, so Build(...) / direct resolution can create it with DI.
            services.TryAddTransient(discoveredType);
        }

        return services;
    }

    public static IServiceCollection AddEavDataBuildFallbacks(this IServiceCollection services)
    {
        // very basic stuff - normally overriden by the platform
        services.TryAddTransient<IValueConverter, ValueConverterUnknown>();

        return services;
    }

}