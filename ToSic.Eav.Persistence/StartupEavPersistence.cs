using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.ImportExport.Sys.Xml;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Serialization.Sys;
using ToSic.Sys.Boot;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.Licenses;
using ToSic.Sys.Utils.Compression;

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

        services.AddTransient<IBootProcess, EavBootLoadFeaturesAndLicenses>();
        services.TryAddTransient<EavFeaturesLoader>();  // new v20 separate class
        services.TryAddTransient<LicenseLoader>();

        services.TryAddTransient<FeaturePersistenceService>();
        services.TryAddTransient<FeaturesIoHelper>();

        return services;
    }

    public static IServiceCollection AddAppStateInFolder(this IServiceCollection services)
    {
        //// core things - usually not replaced
        //services.TryAddTransient<IAppLoader, AppStateInFolderLoader>();
        return services;
    }
}