using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.ImportExport.Internal.Xml;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Internal.Compression;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Serialization.Internal;

namespace ToSic.Eav.Integration;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupEavPersistence
{
    public static IServiceCollection AddEavPersistence(this IServiceCollection services)
    {
        AddAppStateInFolder(services);

        services.TryAddTransient<IDataSerializer, JsonSerializer>();
        services.TryAddTransient<IDataDeserializer, JsonSerializer>();

        services.TryAddTransient<JsonSerializer>();
        services.TryAddTransient<JsonSerializer.MyServices>();
        services.TryAddTransient<SerializerBase.MyServices>();

        services.TryAddTransient<XmlSerializer>();

        services.TryAddTransient<FileSystemLoader>();

        services.TryAddTransient<XmlToEntity>();

        //services.TryAddTransient<FileManager>();

        services.TryAddTransient<EntitySaver>();

        // Compression for SQL History
        services.TryAddTransient<Compressor>();

        services.TryAddTransient<FeaturePersistenceService>();
        services.TryAddTransient<FeaturesIoHelper>();

        return services;
    }

    public static IServiceCollection AddAppStateInFolder(this IServiceCollection services)
    {
        // core things - usually not replaced
        services.TryAddTransient<IAppLoader, AppStateInFolderLoader>();
        return services;
    }
}