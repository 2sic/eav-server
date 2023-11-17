using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Persistence.Xml;
using ToSic.Eav.Run;
using ToSic.Eav.Serialization;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport
{
    public static class StartupImportExport
    {
        public static IServiceCollection AddImportExport(this IServiceCollection services)
        {
            // core things - usually not replaced
            services.TryAddTransient<IAppLoader, AppLoader>();

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

    }
}
