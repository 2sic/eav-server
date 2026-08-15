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
    /// <summary>
    /// Overall add-everything-to-build-data services.
    /// </summary>
    /// <remarks>
    /// Includes
    /// 1. Factories (for raw data)
    /// 1. Content Types
    /// 1. Entities
    ///
    /// Note: ATM also adds EavDataProcessors, but this should be moved elsewhere
    /// </remarks>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddEavDataBuild(this IServiceCollection services)
    {
        // Factories
        services.AddDataBuildFactories();

        // Content Type
        services.AddDataBuildContentTypes();

        // Entities
        services.AddDataBuildEntities();

        services.AddEavDataProcessors();

        return services;
    }

    /// <summary>
    /// Add services for building **Data**, mainly factories.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDataBuildFactories(this IServiceCollection services)
    {
        services.TryAddTransient<IDataFactory, DataFactory>(); // v15.03
        services.TryAddTransient<DataFactoryContentTypeHelper>();

        return services;
    }
    
    /// <summary>
    /// Add services for building **Content Types**, including assemblers and managers.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDataBuildContentTypes(this IServiceCollection services)
    {
        // Basic Assembly of the parts
        services.TryAddTransient<ContentTypeAssembler>();
        services.TryAddTransient<ContentTypeFieldAssembler>();
        
        // Joint Kit to assemble content-types
        services.TryAddTransient<ContentTypeAssemblyKit>();
        
        // Content Types From Code Build and Manage
        services.TryAddTransient<ContentTypesFromCodeBuilder>();
        services.TryAddTransient<ContentTypesFromCodeBuilder.Dependencies>();
        services.TryAddTransient<ContentTypesFromCodeManager>();

        return services;
    }

    /// <summary>
    /// Add services for building **Entities**, including assemblers and connection builders.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDataBuildEntities(this IServiceCollection services)
    {
        // Overall assembler / builder (naming not final/ideal)
        services.TryAddTransient<DataAssembler>();

        // Parts Assemblers
        services.TryAddTransient<LanguageAssembler>();
        services.TryAddTransient<AttributeAssembler>();
        services.TryAddTransient<AttributeListAssembler>();
        services.TryAddTransient<EntityAssembler>();
        services.TryAddTransient<EntityConnectionBuilder>();
        services.TryAddTransient<ValueAssembler>();
        services.TryAddTransient<ValueListAssembler>();
        services.TryAddTransient<RelationshipAssembler>();

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
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IWorkEntityAction), typeof(WorkOnEntityNoOp)));
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IWorkEntityAction), typeof(PermissionDataProcessor)));

        // Register directly, so it can be instantiated
        services.TryAddTransient<PermissionDataProcessor>();

        // Auto-register all concrete IDataProcessor implementations from loaded assemblies (incl. optional bin dlls).
        // Keep this startup-only and dedupe by stable identity to avoid duplicate service descriptors.
        var discoveredTypes = AssemblyHandling
            .FindInherited(typeof(IWorkEntityAction))
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .GroupBy(type => type.FullName ?? type.AssemblyQualifiedName ?? type.Name, StringComparer.Ordinal)
            .Select(group => group.First());

        foreach (var discoveredType in discoveredTypes)
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IWorkEntityAction), discoveredType));

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