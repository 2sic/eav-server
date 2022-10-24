using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Persistence.Xml;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.ImportExport
{
    public static class StartupImportExport
    {
        public static IServiceCollection AddImportExport(this IServiceCollection services)
        {
            services.TryAddTransient<IDataSerializer, JsonSerializer>();
            services.TryAddTransient<IDataDeserializer, JsonSerializer>();

            services.TryAddTransient<JsonSerializer>();

            services.TryAddTransient<XmlSerializer>();

            services.TryAddTransient<FileSystemLoader>();

            services.TryAddTransient<XmlToEntity>();

            return services;
        }

    }
}
