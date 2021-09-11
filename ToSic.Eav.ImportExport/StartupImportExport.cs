using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Convert;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Persistence.Xml;
using ToSic.Eav.Serialization;
using ConvertToJsonBasicBase = ToSic.Eav.ImportExport.Json.Basic.ConvertToJsonBasicBase;

namespace ToSic.Eav.ImportExport
{
    public static class StartupImportExport
    {
        public static IServiceCollection AddImportExport(this IServiceCollection services)
        {
            services.TryAddTransient<IDataSerializer, JsonSerializer>();
            services.TryAddTransient<IDataDeserializer, JsonSerializer>();

            // Here we only register the dependencies, as the final converter must be registered elsewhere
            services.TryAddTransient<ConvertToJsonBasicBase.Dependencies>();


            services.TryAddTransient<JsonSerializer>();

            services.TryAddTransient<XmlSerializer>();

            services.TryAddTransient<FileSystemLoader>();

            // WIP 12.05 - json converter
            services.TryAddTransient<IConvertJson, ConvertJson>();

            return services;
        }

    }
}
