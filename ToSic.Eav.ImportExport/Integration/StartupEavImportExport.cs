using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Internal;
using ToSic.Eav.ImportExport.Internal.Xml;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Serialization.Internal;

namespace ToSic.Eav.Integration;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupEavImportExport
{
    public static IServiceCollection AddImportExport(this IServiceCollection services)
    {
        AddAppLoader(services);

        services.TryAddTransient<IDataSerializer, JsonSerializer>();
        services.TryAddTransient<IDataDeserializer, JsonSerializer>();

        services.TryAddTransient<JsonSerializer>();
        services.TryAddTransient<JsonSerializer.MyServices>();
        services.TryAddTransient<SerializerBase.MyServices>();

        services.TryAddTransient<XmlSerializer>();

        services.TryAddTransient<FileSystemLoader>();

        services.TryAddTransient<XmlToEntity>();

        services.TryAddTransient<FileManager>();

        return services;
    }

    public static IServiceCollection AddAppLoader(this IServiceCollection services)
    {
        // core things - usually not replaced
        services.TryAddTransient<IAppLoader, AppLoader>();
        return services;
    }
}