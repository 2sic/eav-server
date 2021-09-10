using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.ImportExport.Convert.EntityToDictionaryLight;
using ToSic.Eav.ImportExport.Json;
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

            // Here we only register the dependencies, as the final converter must be registered elsewhere
            services.TryAddTransient<EntitiesToDictionaryBase.Dependencies>();


            services.TryAddTransient<JsonSerializer>();

            services.TryAddTransient<XmlSerializer>();

            services.TryAddTransient<FileSystemLoader>();

            return services;
        }

    }
}
